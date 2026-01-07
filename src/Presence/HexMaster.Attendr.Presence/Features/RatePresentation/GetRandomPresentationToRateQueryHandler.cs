using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Presence.Features.RatePresentation;

/// <summary>
/// Query handler to retrieve an unrated presentation at a specific index.
/// Uses deterministic ordering by presentation ID to prevent duplicates.
/// </summary>
public sealed class GetRandomPresentationToRateQueryHandler : IQueryHandler<GetRandomPresentationToRateQuery, PresentationToRateDto?>
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<GetRandomPresentationToRateQueryHandler> _logger;

    public GetRandomPresentationToRateQueryHandler(
        IPresentationPresenceRepository repository,
        PresenceMetrics metrics,
        ILogger<GetRandomPresentationToRateQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PresentationToRateDto?> Handle(GetRandomPresentationToRateQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("GetPresentationToRate", ActivityKind.Internal);
        activity?.SetTag("presence.profile_id", query.ProfileId);
        activity?.SetTag("presence.conference_id", query.ConferenceId);
        activity?.SetTag("presence.index", query.Index);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Getting unrated presentation at index {Index} for profile {ProfileId} and conference {ConferenceId}",
                query.Index,
                query.ProfileId,
                query.ConferenceId);

            var unratedPresentations = await _repository.GetUnratedByProfileAndConferenceAsync(
                query.ProfileId,
                query.ConferenceId,
                cancellationToken);

            activity?.SetTag("presence.unrated_count", unratedPresentations.Count);

            if (unratedPresentations.Count == 0)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(
                    "No unrated presentations found for profile {ProfileId} and conference {ConferenceId}",
                    query.ProfileId,
                    query.ConferenceId);

                _metrics.RecordPresentationQueried(found: false, unratedCount: 0);
                _metrics.RecordOperationDuration("GetPresentationToRate", stopwatch.Elapsed.TotalMilliseconds, true);
                return null;
            }

            // Order presentations by PresentationId for predictable ordering
            var orderedPresentations = unratedPresentations
                .OrderBy(p => p.PresentationId)
                .ToList();

            // Check if the requested index exists
            if (query.Index >= orderedPresentations.Count)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(
                    "Index {Index} is out of range. Only {Count} unrated presentations available for profile {ProfileId} and conference {ConferenceId}",
                    query.Index,
                    orderedPresentations.Count,
                    query.ProfileId,
                    query.ConferenceId);

                _metrics.RecordPresentationQueried(found: false, unratedCount: orderedPresentations.Count);
                _metrics.RecordOperationDuration("GetPresentationToRate", stopwatch.Elapsed.TotalMilliseconds, true);
                return null;
            }

            // Select presentation at the specified index
            var selectedPresentation = orderedPresentations[query.Index];

            activity?.SetTag("presence.selected_presentation_id", selectedPresentation.PresentationId);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _metrics.RecordPresentationQueried(found: true, unratedCount: unratedPresentations.Count);
            _metrics.RecordOperationDuration("GetPresentationToRate", stopwatch.Elapsed.TotalMilliseconds, true);

            var speakers = selectedPresentation.Speakers
                .Select(s => new PresentationSpeakerDto(s.SpeakerId, s.Name, s.ProfilePictureUrl))
                .ToList()
                .AsReadOnly();

            return new PresentationToRateDto(
                selectedPresentation.PresentationId,
                selectedPresentation.Title,
                selectedPresentation.Abstract,
                selectedPresentation.Room,
                selectedPresentation.StartDateTime,
                selectedPresentation.EndDateTime,
                speakers);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("GetPresentationToRate", ex.GetType().Name);
            _metrics.RecordOperationDuration("GetPresentationToRate", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
