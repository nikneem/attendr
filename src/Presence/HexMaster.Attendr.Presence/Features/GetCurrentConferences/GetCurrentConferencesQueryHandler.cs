using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Presence.Features.GetCurrentConferences;

/// <summary>
/// Query handler to retrieve all current conferences for a profile where the profile is following and attending.
/// Returns only conferences where the current date is between start and end date, and the profile is attending.
/// </summary>
public sealed class GetCurrentConferencesQueryHandler : IQueryHandler<GetCurrentConferencesQuery, List<CurrentConferenceResponse>>
{
    private readonly IConferencePresenceRepository _repository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<GetCurrentConferencesQueryHandler> _logger;

    public GetCurrentConferencesQueryHandler(
        IConferencePresenceRepository repository,
        PresenceMetrics metrics,
        ILogger<GetCurrentConferencesQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<CurrentConferenceResponse>> Handle(GetCurrentConferencesQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("GetCurrentConferences", ActivityKind.Internal);
        activity?.SetTag("presence.profile_id", query.ProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Getting current conferences for profile {ProfileId}", query.ProfileId);

            var allPresences = await _repository.GetByProfileIdAsync(query.ProfileId, cancellationToken);
            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);

            // Filter conferences that are:
            // 1. Currently ongoing (today is between start and end date)
            // 2. Profile is attending
            var currentConferences = allPresences
                .Where(p => p.StartDate <= today && p.EndDate >= today && p.IsAttending)
                .OrderBy(p => p.StartDate)
                .Select(p => new CurrentConferenceResponse(
                    p.ConferenceId,
                    p.ConferenceName,
                    p.Location,
                    p.ImageUrl,
                    p.StartDate.ToDateTime(TimeOnly.MinValue),
                    p.EndDate.ToDateTime(TimeOnly.MaxValue)))
                .ToList();

            activity?.SetTag("presence.result_count", currentConferences.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Found {Count} current conferences for profile {ProfileId}",
                currentConferences.Count,
                query.ProfileId);

            _metrics.RecordConferencesQueried(currentConferences.Count);
            _metrics.RecordOperationDuration("GetCurrentConferences", stopwatch.Elapsed.TotalMilliseconds, true);

            return currentConferences;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("GetCurrentConferences", ex.GetType().Name);
            _metrics.RecordOperationDuration("GetCurrentConferences", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
