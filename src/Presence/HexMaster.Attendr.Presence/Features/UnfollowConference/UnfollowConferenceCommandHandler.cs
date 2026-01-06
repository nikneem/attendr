using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Presence.Features.UnfollowConference;

/// <summary>
/// Command handler to unfollow a conference by deleting the conference presence record.
/// This removes all tracking data for the conference, including presentation presences.
/// </summary>
public sealed class UnfollowConferenceCommandHandler : ICommandHandler<UnfollowConferenceCommand>
{
    private readonly IConferencePresenceRepository _repository;
    private readonly IPresentationPresenceRepository _presentationRepository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<UnfollowConferenceCommandHandler> _logger;

    public UnfollowConferenceCommandHandler(
        IConferencePresenceRepository repository,
        IPresentationPresenceRepository presentationRepository,
        PresenceMetrics metrics,
        ILogger<UnfollowConferenceCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _presentationRepository = presentationRepository ?? throw new ArgumentNullException(nameof(presentationRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UnfollowConferenceCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("UnfollowConference", ActivityKind.Internal);
        activity?.SetTag("presence.conference_id", command.ConferenceId);
        activity?.SetTag("presence.profile_id", command.ProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Unfollowing conference {ConferenceId} for profile {ProfileId}",
                command.ConferenceId,
                command.ProfileId);

            // Check if the presence exists
            var exists = await _repository.ExistsAsync(command.ProfileId, command.ConferenceId, cancellationToken);
            if (!exists)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Conference presence not found");
                _logger.LogWarning(
                    "Conference presence not found for profile {ProfileId} and conference {ConferenceId}",
                    command.ProfileId,
                    command.ConferenceId);
                throw new InvalidOperationException("You are not following this conference.");
            }

            // Delete all presentation presences for this conference and profile
            // Note: This could be optimized with a bulk delete operation if needed
            var presentations = await _presentationRepository.GetByProfileAndConferenceAsync(
                command.ProfileId,
                command.ConferenceId,
                cancellationToken);

            foreach (var presentation in presentations)
            {
                await _presentationRepository.DeleteAsync(
                    command.ProfileId,
                    command.ConferenceId,
                    presentation.PresentationId,
                    cancellationToken);
            }

            _logger.LogDebug(
                "Deleted {Count} presentation presences for profile {ProfileId} and conference {ConferenceId}",
                presentations.Count,
                command.ProfileId,
                command.ConferenceId);

            // Delete the conference presence
            await _repository.DeleteAsync(command.ConferenceId, command.ProfileId, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("UnfollowConference", stopwatch.Elapsed.TotalMilliseconds, true);

            _logger.LogInformation(
                "Successfully unfollowed conference {ConferenceId} for profile {ProfileId}",
                command.ConferenceId,
                command.ProfileId);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("UnfollowConference", ex.GetType().Name);
            _metrics.RecordOperationDuration("UnfollowConference", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
