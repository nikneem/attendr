using HexMaster.Attendr.Conferences.Integrations.Abstractions;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Services;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.CreateConferencePresence;

/// <summary>
/// Command handler to create conference presence records for profiles.
/// Creates presence tracking when users follow a conference.
/// </summary>
public sealed class CreateConferencePresenceCommandHandler : ICommandHandler<CreateConferencePresenceCommand>
{
    private readonly IConferencesIntegrationService _conferencesIntegration;
    private readonly IConferencePresenceRepository _repository;
    private readonly ILogger<CreateConferencePresenceCommandHandler> _logger;

    public CreateConferencePresenceCommandHandler(
        IConferencesIntegrationService conferencesIntegration,
        IConferencePresenceRepository repository,
        ILogger<CreateConferencePresenceCommandHandler> logger)
    {
        _conferencesIntegration = conferencesIntegration ?? throw new ArgumentNullException(nameof(conferencesIntegration));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(CreateConferencePresenceCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command.ProfileIds);

        // Fetch conference details once
        var details = await _conferencesIntegration.GetConferenceDetails(command.ConferenceId, cancellationToken);
        if (details is null)
        {
            _logger.LogWarning("Conference {ConferenceId} not found", command.ConferenceId);
            throw new InvalidOperationException($"Conference {command.ConferenceId} not found");
        }

        // Create presence records for each profile
        foreach (var profileId in command.ProfileIds)
        {
            var exists = await _repository.ExistsAsync(profileId, command.ConferenceId, cancellationToken);
            if (exists)
            {
                _logger.LogDebug(
                    "Presence already exists for profile {ProfileId} and conference {ConferenceId}",
                    profileId,
                    command.ConferenceId);
                continue;
            }

            var presence = new ConferencePresence(
                command.ConferenceId,
                details.Title.ToString(),
                $"{details.City}, {details.Country}",
                DateOnly.Parse(details.StartDate.ToString()),
                DateOnly.Parse(details.EndDate.ToString()),
                profileId,
                isFollowing: true,
                isAttending: false,
                presentations: null);

            await _repository.AddAsync(presence, cancellationToken);

            _logger.LogInformation(
                "Created conference presence for profile {ProfileId} and conference {ConferenceId}",
                profileId,
                command.ConferenceId);
        }
    }
}
