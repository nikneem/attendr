using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Conferences.GetConference;

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

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var conference = await _conferenceRepository.GetByIdAsync(query.ConferenceId, cancellationToken);

            if (conference == null)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                activity?.SetTag("conference.found", false);
                _metrics.RecordConferenceQueried(found: false);
                _metrics.RecordOperationDuration("GetConference", stopwatch.Elapsed.TotalMilliseconds, success: true);

                _logger.LogInformation("Conference {ConferenceId} not found", query.ConferenceId);
                return null;
            }

            activity?.SetTag("conference.found", true);
            activity?.SetTag("conference.title", conference.Title);
            activity?.SetTag("conference.has_sync_source", conference.SynchronizationSource is not null);

            SynchronizationSourceDto? syncSourceDto = null;
            if (conference.SynchronizationSource != null)
            {
                syncSourceDto = new SynchronizationSourceDto(
                    conference.SynchronizationSource.SourceType.ToString(),
                    conference.SynchronizationSource.SourceLocationOrApiKey ?? string.Empty);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordConferenceQueried(found: true);
            _metrics.RecordOperationDuration("GetConference", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Retrieved conference {ConferenceId}: {Title}", conference.Id, conference.Title);

            return new ConferenceDetailsDto(
                conference.Id,
                conference.Title,
                conference.City,
                conference.Country,
                conference.StartDate,
                conference.EndDate,
                conference.ImageUrl,
                syncSourceDto);
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
