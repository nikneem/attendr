using System.Diagnostics;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.DeleteTopic;

/// <summary>
/// Command handler to delete a topic.
/// When a topic is deleted, all references to presentations are also deleted (cascade delete).
/// </summary>
public sealed class DeleteTopicCommandHandler : ICommandHandler<DeleteTopicCommand, bool>
{
    private readonly ITopicsRepository _topicsRepository;
    private readonly ConferenceMetrics _metrics;
    private readonly ILogger<DeleteTopicCommandHandler> _logger;

    public DeleteTopicCommandHandler(
        ITopicsRepository topicsRepository,
        ConferenceMetrics metrics,
        ILogger<DeleteTopicCommandHandler> logger)
    {
        _topicsRepository = topicsRepository ?? throw new ArgumentNullException(nameof(topicsRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(DeleteTopicCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Conferences.StartActivity("DeleteTopic", ActivityKind.Internal);
        activity?.SetTag("topic.id", command.Id);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Deleting topic {TopicId}", command.Id);

            // Cascade delete: Remove all references to presentations first
            await _topicsRepository.DeleteTopicPresentationReferencesAsync(command.Id, cancellationToken);

            // Then delete the topic itself
            var deleted = await _topicsRepository.DeleteTopicAsync(command.Id, cancellationToken);

            if (!deleted)
            {
                _logger.LogWarning("Topic {TopicId} not found for deletion", command.Id);
                activity?.SetStatus(ActivityStatusCode.Error, "Topic not found");
                _metrics.RecordOperationFailed("DeleteTopic", "NotFound");
                _metrics.RecordOperationDuration("DeleteTopic", stopwatch.Elapsed.TotalMilliseconds, success: false);
                return false;
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("DeleteTopic", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Topic {TopicId} deleted successfully", command.Id);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("DeleteTopic", ex.GetType().Name);
            _metrics.RecordOperationDuration("DeleteTopic", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to delete topic {TopicId}", command.Id);
            throw;
        }
    }
}
