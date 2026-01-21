using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ListTopics;

/// <summary>
/// Query handler for listing topics.
/// </summary>
public sealed class ListTopicsQueryHandler : IQueryHandler<ListTopicsQuery, ListTopicsResult>
{
    private readonly ITopicsRepository _topicsRepository;
    private readonly ConferenceMetrics _metrics;
    private readonly ILogger<ListTopicsQueryHandler> _logger;

    public ListTopicsQueryHandler(
        ITopicsRepository topicsRepository,
        ConferenceMetrics metrics,
        ILogger<ListTopicsQueryHandler> logger)
    {
        _topicsRepository = topicsRepository ?? throw new ArgumentNullException(nameof(topicsRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ListTopicsResult> Handle(ListTopicsQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Conferences.StartActivity("ListTopics", ActivityKind.Internal);
        activity?.SetTag("topics.only_visible", query.OnlyVisible);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var topics = await _topicsRepository.ListTopicsAsync(query.OnlyVisible, cancellationToken);

            var topicDtos = topics
                .OrderBy(t => t.Key)
                .Select(t => new TopicDto(t.Id, t.Key, t.Name, t.IsVisible))
                .ToList();

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("topics.count", topicDtos.Count);
            _metrics.RecordOperationDuration("ListTopics", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Listed {Count} topics", topicDtos.Count);

            return new ListTopicsResult(topicDtos, topicDtos.Count);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("ListTopics", ex.GetType().Name);
            _metrics.RecordOperationDuration("ListTopics", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to list topics");
            throw;
        }
    }
}
