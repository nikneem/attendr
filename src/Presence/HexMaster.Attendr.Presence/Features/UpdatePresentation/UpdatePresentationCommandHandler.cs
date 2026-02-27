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
/// Creates a new presentation presence when one does not yet exist for an attending profile.
/// Publishes PresentationScheduleChangeEvent when schedule changes affect favorited presentations.
/// </summary>
public sealed class UpdatePresentationCommandHandler : ICommandHandler<UpdatePresentationCommand>
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly IConferencePresenceRepository _conferencePresenceRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<UpdatePresentationCommandHandler> _logger;

    public UpdatePresentationCommandHandler(
        IPresentationPresenceRepository repository,
        IConferencePresenceRepository conferencePresenceRepository,
        IIntegrationEventPublisher eventPublisher,
        PresenceMetrics metrics,
        ILogger<UpdatePresentationCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _conferencePresenceRepository = conferencePresenceRepository ?? throw new ArgumentNullException(nameof(conferencePresenceRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UpdatePresentationCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("UpdatePresentation", ActivityKind.Internal);

        var stopwatch = Stopwatch.StartNew();
        var scheduleChanged = false;
        var createdCount = 0;
        var updatedCount = 0;

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

            // Get all profiles attending the conference
            var conferencePresences = await _conferencePresenceRepository.GetByConferenceIdAsync(
                @event.ConferenceId,
                cancellationToken);

            if (conferencePresences.Count == 0)
            {
                _logger.LogInformation(
                    "No conference attendees found for Conference {ConferenceId}, skipping presentation update",
                    @event.ConferenceId);
                return;
            }

            var speakers = @event.Speakers
                .Select(s => new PresentationSpeaker(s.Id, s.Name, s.ProfilePictureUrl ?? string.Empty))
                .ToList();

            var topics = @event.Topics
                .Select(t => new PresentationTopic(t.Key, t.Name))
                .ToList();

            foreach (var conferencePresence in conferencePresences)
            {
                var profileId = conferencePresence.ProfileId;

                // Get the existing presentation presence for this profile, or null if not yet created
                var presentation = await _repository.GetByConferenceAndPresentationAsync(
                    profileId,
                    @event.ConferenceId,
                    @event.PresentationId,
                    cancellationToken);

                if (presentation == null)
                {
                    // Presentation does not exist for this profile — create it
                    var newPresentation = new PresentationPresence(
                        profileId,
                        @event.ConferenceId,
                        @event.PresentationId,
                        @event.Title,
                        @event.Abstract,
                        @event.RoomName,
                        @event.StartDateTime,
                        @event.EndDateTime,
                        speakers,
                        topics);

                    await _repository.AddAsync(newPresentation, cancellationToken);
                    createdCount++;

                    _logger.LogInformation(
                        "Created new presentation presence for Profile {ProfileId}, Conference {ConferenceId}, Presentation {PresentationId}",
                        profileId,
                        @event.ConferenceId,
                        @event.PresentationId);
                }
                else
                {
                    // Presentation already exists — preserve existing speaker info when available
                    var updatedSpeakers = @event.Speakers
                        .Select(s => new PresentationSpeaker(s.Id, s.Name, s.ProfilePictureUrl))
                        .ToList();

                    // Update presentation info (preserves IsFavorite, IsCheckedIn, IsRated, Rating)
                    presentation.UpdatePresentationInfo(
                        @event.Title,
                        @event.Abstract,
                        @event.RoomName,
                        @event.StartDateTime,
                        @event.EndDateTime,
                        updatedSpeakers,
                        topics);

                    await _repository.UpdateAsync(
                        profileId,
                        @event.ConferenceId,
                        presentation,
                        cancellationToken);

                    updatedCount++;

                    // If schedule changed and presentation is favorited, raise schedule change event
                    if (@event.IsScheduleChanged && presentation.IsFavorite)
                    {
                        scheduleChanged = true;
                        var scheduleChangeEvent = new PresentationScheduleChangeEvent
                        {
                            ConferenceId = @event.ConferenceId,
                            PresentationId = @event.PresentationId,
                            ProfileId = profileId,
                            Title = @event.Title,
                            Abstract = @event.Abstract,
                            Room = @event.RoomName,
                            StartDateTime = @event.StartDateTime,
                            EndDateTime = @event.EndDateTime
                        };

                        await _eventPublisher.PublishAsync(scheduleChangeEvent, cancellationToken);

                        _logger.LogInformation(
                            "Published PresentationScheduleChangeEvent for Profile {ProfileId}, Conference {ConferenceId}, Presentation {PresentationId}",
                            profileId,
                            @event.ConferenceId,
                            @event.PresentationId);
                    }
                }
            }

            _logger.LogInformation(
                "Successfully processed presentation update for Conference {ConferenceId}, Presentation {PresentationId}: {CreatedCount} created, {UpdatedCount} updated",
                @event.ConferenceId,
                @event.PresentationId,
                createdCount,
                updatedCount);

            activity?.SetTag("presence.created_count", createdCount);
            activity?.SetTag("presence.updated_count", updatedCount);
            activity?.SetTag("presence.schedule_changed", scheduleChanged);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _metrics.RecordPresentationUpdated(updatedCount, scheduleChanged);
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
