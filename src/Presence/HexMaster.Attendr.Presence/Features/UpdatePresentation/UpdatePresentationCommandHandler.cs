using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Services;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.UpdatePresentation;

/// <summary>
/// Command handler to update presentation information across all affected presentation presences.
/// Publishes PresentationScheduleChangeEvent when schedule changes affect favorited presentations.
/// </summary>
public sealed class UpdatePresentationCommandHandler : ICommandHandler<UpdatePresentationCommand>
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<UpdatePresentationCommandHandler> _logger;

    public UpdatePresentationCommandHandler(
        IPresentationPresenceRepository repository,
        IIntegrationEventPublisher eventPublisher,
        PresenceMetrics metrics,
        ILogger<UpdatePresentationCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UpdatePresentationCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("UpdatePresentation", ActivityKind.Internal);

        var stopwatch = Stopwatch.StartNew();
        var scheduleChanged = false;

        try
        {
            ArgumentNullException.ThrowIfNull(command.Event);

            var @event = command.Event;

            activity?.SetTag("presence.conference_id", @event.ConferenceId);
            activity?.SetTag("presence.presentation_id", @event.PresentationId);

            _logger.LogInformation(
                "Handling PresentationUpdatedEvent for Conference {ConferenceId}, Presentation {PresentationId}",
                @event.ConferenceId,
                @event.PresentationId);

            // Get all presentation presences for this conference and presentation
            var presentations = await _repository.GetByConferenceAndPresentationAsync(
                @event.ConferenceId,
                @event.PresentationId,
                cancellationToken);

            if (presentations.Count == 0)
            {
                _logger.LogInformation(
                    "No presentation presences found for Conference {ConferenceId}, Presentation {PresentationId}",
                    @event.ConferenceId,
                    @event.PresentationId);
                return;
            }

            _logger.LogInformation(
                "Found {Count} presentation presence(s) to update for Conference {ConferenceId}, Presentation {PresentationId}",
                presentations.Count,
                @event.ConferenceId,
                @event.PresentationId);

            // Update each presentation presence
            foreach (var presentation in presentations)
            {
                // Create speakers list (preserve existing speaker info when available)
                var currentSpeakers = presentation.Speakers.ToDictionary(s => s.SpeakerId);
                var speakers = @event.SpeakerIds
                    .Select(id =>
                    {
                        if (currentSpeakers.TryGetValue(id, out var existingSpeaker))
                        {
                            return existingSpeaker;
                        }
                        return new PresentationSpeaker(id, string.Empty, string.Empty);
                    })
                    .ToList();

                // Update presentation info (preserves IsFavorite, IsCheckedIn, IsRated, Rating)
                presentation.UpdatePresentationInfo(
                    @event.Title,
                    @event.Abstract,
                    @event.RoomName,
                    @event.StartDateTime,
                    @event.EndDateTime,
                    speakers);

                // Save the updated presentation
                await _repository.UpdateAsync(
                    presentation.ProfileId,
                    @event.ConferenceId,
                    presentation,
                    cancellationToken);

                // If schedule changed and presentation is favorited, raise schedule change event
                if (@event.IsScheduleChanged && presentation.IsFavorite)
                {
                    scheduleChanged = true;
                    var scheduleChangeEvent = new PresentationScheduleChangeEvent
                    {
                        ConferenceId = @event.ConferenceId,
                        PresentationId = @event.PresentationId,
                        ProfileId = presentation.ProfileId,
                        Title = @event.Title,
                        Abstract = @event.Abstract,
                        Room = @event.RoomName,
                        StartDateTime = @event.StartDateTime,
                        EndDateTime = @event.EndDateTime
                    };

                    await _eventPublisher.PublishAsync(scheduleChangeEvent, cancellationToken);

                    _logger.LogInformation(
                        "Published PresentationScheduleChangeEvent for Profile {ProfileId}, Conference {ConferenceId}, Presentation {PresentationId}",
                        scheduleChangeEvent.ProfileId,
                        @event.ConferenceId,
                        @event.PresentationId);
                }
            }

            _logger.LogInformation(
                "Successfully updated {Count} presentation presence(s) for Conference {ConferenceId}, Presentation {PresentationId}",
                presentations.Count,
                @event.ConferenceId,
                @event.PresentationId);

            activity?.SetTag("presence.affected_count", presentations.Count);
            activity?.SetTag("presence.schedule_changed", scheduleChanged);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _metrics.RecordPresentationUpdated(presentations.Count, scheduleChanged);
            _metrics.RecordOperationDuration("UpdatePresentation", stopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("UpdatePresentation", ex.GetType().Name);
            _metrics.RecordOperationDuration("UpdatePresentation", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
