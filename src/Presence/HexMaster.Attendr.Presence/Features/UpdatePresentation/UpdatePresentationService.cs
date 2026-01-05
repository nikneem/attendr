using Microsoft.Extensions.Logging;
using HexMaster.Attendr.IntegrationEvents.Events;
using HexMaster.Attendr.IntegrationEvents.Services;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Services;

namespace HexMaster.Attendr.Presence.Features.UpdatePresentation;

public sealed class UpdatePresentationService
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<UpdatePresentationService> _logger;

    public UpdatePresentationService(
        IPresentationPresenceRepository repository,
        IIntegrationEventPublisher eventPublisher,
        ILogger<UpdatePresentationService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(
        PresentationUpdatedEvent @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

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
            // Create speakers list (we don't have full speaker info in the event, so we keep speaker IDs and preserve names/pictures)
            var currentSpeakers = presentation.Speakers.ToDictionary(s => s.SpeakerId);
            var speakers = @event.SpeakerIds
                .Select(id =>
                {
                    // Preserve existing speaker info if available, otherwise create with empty values
                    if (currentSpeakers.TryGetValue(id, out var existingSpeaker))
                    {
                        return existingSpeaker;
                    }
                    return new PresentationSpeaker(id, string.Empty, string.Empty);
                })
                .ToList();

            // Update presentation info (preserving IsFavorite, IsCheckedIn, IsRated, Rating)
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
    }
}

