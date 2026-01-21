using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.GetTopic;

/// <summary>
/// Query handler for retrieving a specific topic by ID.
/// </summary>
public sealed class GetTopicQueryHandler : IQueryHandler<GetTopicQuery, TopicDto?>
{
    private readonly ITopicsRepository _topicsRepository;
    private readonly ConferenceMetrics _metrics;
    private readonly ILogger<GetTopicQueryHandler> _logger;

    public GetTopicQueryHandler(
        ITopicsRepository topicsRepository,
        ConferenceMetrics metrics,
        ILogger<GetTopicQueryHandler> logger)
    {
        _topicsRepository = topicsRepository ?? throw new ArgumentNullException(nameof(topicsRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TopicDto?> Handle(GetTopicQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Conferences.StartActivity("GetTopic", ActivityKind.Internal);
        activity?.SetTag("topic.id", query.TopicId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Note: ITopicsRepository doesn't have a GetByIdAsync method yet
            // This is a placeholder that assumes it will be added
            // For now, this will need to be implemented in the repository
            var topic = await _topicsRepository.GetTopicByIdAsync(query.TopicId, cancellationToken);

            if (topic == null)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                activity?.SetTag("topic.found", false);
                _metrics.RecordOperationDuration("GetTopic", stopwatch.Elapsed.TotalMilliseconds, success: true);
                _logger.LogInformation("Topic {TopicId} not found", query.TopicId);
                return null;
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("topic.found", true);
            activity?.SetTag("topic.key", topic.Key);
            _metrics.RecordOperationDuration("GetTopic", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Retrieved topic {TopicId}: {Key}", topic.Id, topic.Key);

            return new TopicDto(topic.Id, topic.Key, topic.Name, topic.IsVisible);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("GetTopic", ex.GetType().Name);
            _metrics.RecordOperationDuration("GetTopic", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to retrieve topic {TopicId}", query.TopicId);
            throw;
        }
    }
}
