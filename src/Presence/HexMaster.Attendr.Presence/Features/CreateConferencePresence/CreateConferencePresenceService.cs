using Microsoft.Extensions.Logging;
using HexMaster.Attendr.Conferences.Integrations.Abstractions;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Services;

namespace HexMaster.Attendr.Presence.Features.CreateConferencePresence;

public sealed class CreateConferencePresenceService
{
    private readonly IConferencesIntegrationService _conferencesIntegration;
    private readonly IConferencePresenceRepository _repository;
    private readonly ILogger<CreateConferencePresenceService> _logger;

    public CreateConferencePresenceService(
        IConferencesIntegrationService conferencesIntegration,
        IConferencePresenceRepository repository,
        ILogger<CreateConferencePresenceService> logger)
    {
        _conferencesIntegration = conferencesIntegration ?? throw new ArgumentNullException(nameof(conferencesIntegration));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(
        Guid conferenceId,
        IEnumerable<Guid> profileIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profileIds);

        // Fetch conference details once
        var details = await _conferencesIntegration.GetConferenceDetails(conferenceId, cancellationToken);
        if (details is null)
        {
            _logger.LogWarning("Conference {ConferenceId} not found", conferenceId);
            throw new InvalidOperationException($"Conference {conferenceId} not found");
        }

        // Note: Presentations will be created separately for each profile

        // Create presence records for each profile
        foreach (var profileId in profileIds)
        {
            var exists = await _repository.ExistsAsync(profileId, conferenceId, cancellationToken);
            if (exists)
            {
                _logger.LogDebug(
                    "Presence already exists for profile {ProfileId} and conference {ConferenceId}",
                    profileId,
                    conferenceId);
                continue;
            }

            var presence = new ConferencePresence(
                conferenceId,
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
                conferenceId);
        }
    }
}

