using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.IntegrationEvents.Events;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Conferences.FollowConference;

/// <summary>
/// Command handler to allow a profile to follow a conference.
/// Publishes an integration event to notify other services.
/// </summary>
public sealed class FollowConferenceCommandHandler : ICommandHandler<FollowConferenceCommand>
{
    private readonly IConferenceRepository _conferenceRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<FollowConferenceCommandHandler> _logger;

    public FollowConferenceCommandHandler(
        IConferenceRepository conferenceRepository,
        IIntegrationEventPublisher eventPublisher,
        ILogger<FollowConferenceCommandHandler> logger)
    {
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(FollowConferenceCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Conferences.StartActivity("FollowConference", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.ConferenceId);
        activity?.SetTag("profile.id", command.ProfileId);

        try
        {
            var conference = await _conferenceRepository.GetByIdAsync(command.ConferenceId, cancellationToken);

            if (conference is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Conference not found");
                _logger.LogWarning("Attempted to follow non-existent conference {ConferenceId}", command.ConferenceId);
                throw new KeyNotFoundException($"Conference with ID {command.ConferenceId} not found.");
            }

            activity?.SetTag("conference.title", conference.Title);

            var profileFollowedConferenceEvent = new ProfileFollowedConferenceEvent
            {
                ConferenceId = command.ConferenceId,
                ProfileId = command.ProfileId
            };

            await _eventPublisher.PublishAsync(profileFollowedConferenceEvent, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation("Profile {ProfileId} is now following conference {ConferenceId} ({ConferenceTitle})",
                command.ProfileId, command.ConferenceId, conference.Title);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);

            _logger.LogError(ex, "Failed to follow conference {ConferenceId} for profile {ProfileId}",
                command.ConferenceId, command.ProfileId);
            throw;
        }
    }
}
