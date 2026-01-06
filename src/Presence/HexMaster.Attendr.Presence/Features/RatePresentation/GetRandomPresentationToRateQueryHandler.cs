using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Observability;
using HexMaster.Attendr.Presence.Services;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Presence.Features.RatePresentation;

/// <summary>
/// Query handler to retrieve a random unrated presentation for rating.
/// Helps users discover presentations they haven't rated yet.
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
        using var activity = ActivitySources.Presence.StartActivity("GetRandomPresentationToRate", ActivityKind.Internal);
        activity?.SetTag("presence.profile_id", query.ProfileId);
        activity?.SetTag("presence.conference_id", query.ConferenceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Getting random unrated presentation for profile {ProfileId} and conference {ConferenceId}",
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
                _metrics.RecordOperationDuration("GetRandomPresentationToRate", stopwatch.Elapsed.TotalMilliseconds, true);
                return null;
            }

            // Select random presentation
            var random = new Random();
            var randomPresentation = unratedPresentations.ElementAt(random.Next(unratedPresentations.Count));

            activity?.SetTag("presence.selected_presentation_id", randomPresentation.PresentationId);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _metrics.RecordPresentationQueried(found: true, unratedCount: unratedPresentations.Count);
            _metrics.RecordOperationDuration("GetRandomPresentationToRate", stopwatch.Elapsed.TotalMilliseconds, true);

            return new PresentationToRateDto(
                randomPresentation.PresentationId,
                randomPresentation.Title,
                randomPresentation.Abstract,
                randomPresentation.Room,
                randomPresentation.StartDateTime,
                randomPresentation.EndDateTime);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("GetRandomPresentationToRate", ex.GetType().Name);
            _metrics.RecordOperationDuration("GetRandomPresentationToRate", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
