using System.Diagnostics;
using HexMaster.Attendr.Conferences.Integrations.Abstractions;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Observability;
using HexMaster.Attendr.Presence.Services;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Presence.Features.CreateConferencePresence;

/// <summary>
/// Command handler to create conference presence records for profiles.
/// Creates presence tracking when users follow a conference.
/// </summary>
public sealed class CreateConferencePresenceCommandHandler : ICommandHandler<CreateConferencePresenceCommand>
{
    private readonly IConferencesIntegrationService _conferencesIntegration;
    private readonly IConferencePresenceRepository _repository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<CreateConferencePresenceCommandHandler> _logger;

    public CreateConferencePresenceCommandHandler(
        IConferencesIntegrationService conferencesIntegration,
        IConferencePresenceRepository repository,
        PresenceMetrics metrics,
        ILogger<CreateConferencePresenceCommandHandler> logger)
    {
        _conferencesIntegration = conferencesIntegration ?? throw new ArgumentNullException(nameof(conferencesIntegration));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(CreateConferencePresenceCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("CreateConferencePresence", ActivityKind.Internal);
        activity?.SetTag("presence.conference_id", command.ConferenceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            ArgumentNullException.ThrowIfNull(command.ProfileIds);

            var profileList = command.ProfileIds.ToList();
            var profileCount = profileList.Count;
            activity?.SetTag("presence.profile_count", profileCount);

            // Fetch conference details once
            var details = await _conferencesIntegration.GetConferenceDetails(command.ConferenceId, cancellationToken);
            if (details is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Conference not found");
                _logger.LogWarning("Conference {ConferenceId} not found", command.ConferenceId);
                throw new InvalidOperationException($"Conference {command.ConferenceId} not found");
            }

            // Create presence records for each profile
            foreach (var profileId in profileList)
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

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordPresenceCreated(profileCount);
            _metrics.RecordOperationDuration("CreateConferencePresence", stopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("CreateConferencePresence", ex.GetType().Name);
            _metrics.RecordOperationDuration("CreateConferencePresence", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
