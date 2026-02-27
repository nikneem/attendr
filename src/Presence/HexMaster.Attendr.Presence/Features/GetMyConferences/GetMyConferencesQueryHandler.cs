using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.GetMyConferences;

/// <summary>
/// Query handler to retrieve all current and future conferences for a profile.
/// Returns conferences the user is following, ordered by start date.
/// </summary>
public sealed class GetMyConferencesQueryHandler : IQueryHandler<GetMyConferencesQuery, List<MyConferenceResponse>>
{
    private readonly IConferencePresenceRepository _repository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<GetMyConferencesQueryHandler> _logger;

    public GetMyConferencesQueryHandler(
        IConferencePresenceRepository repository,
        PresenceMetrics metrics,
        ILogger<GetMyConferencesQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<MyConferenceResponse>> Handle(GetMyConferencesQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("GetMyConferences", ActivityKind.Internal);
        activity?.SetTag("presence.profile_id", query.ProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Getting conferences for profile {ProfileId}", query.ProfileId);

            var allPresences = await _repository.GetByProfileIdAsync(query.ProfileId, cancellationToken);
            var now = DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime);

            var currentAndFuture = allPresences
                .Where(p => p.EndDate >= now)
                .OrderBy(p => p.StartDate)
                .Select(p => new MyConferenceResponse(
                    p.ConferenceId,
                    p.ConferenceName,
                    p.Location,
                    p.ImageUrl,
                    p.StartDate,
                    p.EndDate,
                    p.IsAttending))
                .ToList();

            activity?.SetTag("presence.result_count", currentAndFuture.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Found {Count} current/future conferences for profile {ProfileId}",
                currentAndFuture.Count,
                query.ProfileId);

            _metrics.RecordConferencesQueried(currentAndFuture.Count);
            _metrics.RecordOperationDuration("GetMyConferences", stopwatch.Elapsed.TotalMilliseconds, true);

            return currentAndFuture;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("GetMyConferences", ex.GetType().Name);
            _metrics.RecordOperationDuration("GetMyConferences", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
