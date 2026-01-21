using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.CreateTopic;

/// <summary>
/// Command handler to create a new topic.
/// </summary>
public sealed class CreateTopicCommandHandler : ICommandHandler<CreateTopicCommand, TopicDto>
{
    private readonly ITopicsRepository _topicsRepository;
    private readonly ConferenceMetrics _metrics;
    private readonly ILogger<CreateTopicCommandHandler> _logger;

    public CreateTopicCommandHandler(
        ITopicsRepository topicsRepository,
        ConferenceMetrics metrics,
        ILogger<CreateTopicCommandHandler> logger)
    {
        _topicsRepository = topicsRepository ?? throw new ArgumentNullException(nameof(topicsRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TopicDto> Handle(CreateTopicCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Conferences.StartActivity("CreateTopic", ActivityKind.Internal);
        activity?.SetTag("topic.key", command.Key);
        activity?.SetTag("topic.name", command.Name);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Creating topic with key: {Key}, name: {Name}", command.Key, command.Name);

            var topic = Topic.Create(command.Key, command.Name);

            // Note: The repository should implement adding the topic
            // This assumes the repository has an AddAsync method or similar
            // For now, we'll persist through GetOrCreateTopicAsync as a workaround
            var createdTopic = await _topicsRepository.GetOrCreateTopicAsync(
                topic.Key,
                topic.Name,
                cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("topic.id", createdTopic.Id);
            _metrics.RecordOperationDuration("CreateTopic", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Topic {TopicId} created successfully with key {Key}", createdTopic.Id, createdTopic.Key);

            return new TopicDto(createdTopic.Id, createdTopic.Key, createdTopic.Name, createdTopic.IsVisible);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("CreateTopic", ex.GetType().Name);
            _metrics.RecordOperationDuration("CreateTopic", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to create topic with key {Key}", command.Key);
            throw;
        }
    }
}
