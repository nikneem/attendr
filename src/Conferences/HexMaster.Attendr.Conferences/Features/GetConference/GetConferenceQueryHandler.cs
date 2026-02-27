using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.GetConference;

/// <summary>
/// Query handler for retrieving a specific conference by ID.
/// Implements distributed tracing via OpenTelemetry and structured logging.
/// </summary>
public sealed class GetConferenceQueryHandler : IQueryHandler<GetConferenceQuery, ConferenceDetailsDto?>
{
    private readonly IConferenceRepository _conferenceRepository;
    private readonly ConferenceMetrics _metrics;
    private readonly ILogger<GetConferenceQueryHandler> _logger;

    public GetConferenceQueryHandler(
        IConferenceRepository conferenceRepository,
        ConferenceMetrics metrics,
        ILogger<GetConferenceQueryHandler> logger)
    {
        _conferenceRepository = conferenceRepository;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<ConferenceDetailsDto?> Handle(GetConferenceQuery query, CancellationToken cancellationToken)
    {
        using var activity = ActivitySources.Conferences.StartActivity("GetConference", ActivityKind.Internal);
        activity?.SetTag("conference.id", query.ConferenceId);
        if (query.CurrentProfileId.HasValue)
        {
            activity?.SetTag("conference.current_profile_id", query.CurrentProfileId.Value);
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var conferenceDetails = await _conferenceRepository.GetDetailsByIdAsync(query.ConferenceId, query.CurrentProfileId, cancellationToken);

            if (conferenceDetails == null)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                activity?.SetTag("conference.found", false);
                _metrics.RecordConferenceQueried(found: false);
                _metrics.RecordOperationDuration("GetConference", stopwatch.Elapsed.TotalMilliseconds, success: true);

                _logger.LogInformation("Conference {ConferenceId} not found", query.ConferenceId);
                return null;
            }

            activity?.SetTag("conference.found", true);
            activity?.SetTag("conference.title", conferenceDetails.Title);
            activity?.SetTag("conference.has_sync_source", conferenceDetails.SynchronizationSource is not null);

            // Debug log when an invisible conference is returned to its owner
            if (!conferenceDetails.IsVisible && query.CurrentProfileId.HasValue)
            {
                _logger.LogDebug(
                    "Conference {ConferenceId} is not visible but returned to owner (profile {ProfileId})",
                    conferenceDetails.Id, query.CurrentProfileId.Value);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordConferenceQueried(found: true);
            _metrics.RecordOperationDuration("GetConference", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Retrieved conference {ConferenceId}: {Title}", conferenceDetails.Id, conferenceDetails.Title);

            return conferenceDetails;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("GetConference", ex.GetType().Name);
            _metrics.RecordOperationDuration("GetConference", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to retrieve conference {ConferenceId}", query.ConferenceId);
            throw;
        }
    }
}
