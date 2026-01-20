using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.ResetConferenceRatings;

/// <summary>
/// Command handler to reset ratings for all presentations of a conference.
/// Sets IsRated to false, IsFavorite to false, and Rating to null.
/// </summary>
public sealed class ResetConferenceRatingsCommandHandler : ICommandHandler<ResetConferenceRatingsCommand>
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<ResetConferenceRatingsCommandHandler> _logger;

    public ResetConferenceRatingsCommandHandler(
        IPresentationPresenceRepository repository,
        PresenceMetrics metrics,
        ILogger<ResetConferenceRatingsCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(ResetConferenceRatingsCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("ResetConferenceRatings", ActivityKind.Internal);
        activity?.SetTag("presence.profile_id", command.ProfileId);
        activity?.SetTag("presence.conference_id", command.ConferenceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Resetting ratings for all presentations in conference {ConferenceId} for profile {ProfileId}",
                command.ConferenceId,
                command.ProfileId);

            var affectedCount = await _repository.ResetRatingsAsync(
                command.ProfileId,
                command.ConferenceId,
                cancellationToken);

            activity?.SetTag("presence.affected_count", affectedCount);

            _logger.LogInformation(
                "Successfully reset ratings for {AffectedCount} presentations in conference {ConferenceId} for profile {ProfileId}",
                affectedCount,
                command.ConferenceId,
                command.ProfileId);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("ResetConferenceRatings", stopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("ResetConferenceRatings", ex.GetType().Name);
            _metrics.RecordOperationDuration("ResetConferenceRatings", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
