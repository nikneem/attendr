using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.IntegrationEvents.Events.Topics;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.CreateTopic;

/// <summary>
/// Command handler to create a new topic.
/// </summary>
public sealed class CreateTopicCommandHandler : ICommandHandler<CreateTopicCommand, TopicDto>
{
    private readonly ITopicsRepository _topicsRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ConferenceMetrics _metrics;
    private readonly ILogger<CreateTopicCommandHandler> _logger;

    public CreateTopicCommandHandler(
        ITopicsRepository topicsRepository,
        IIntegrationEventPublisher eventPublisher,
        ConferenceMetrics metrics,
        ILogger<CreateTopicCommandHandler> logger)
    {
        _topicsRepository = topicsRepository ?? throw new ArgumentNullException(nameof(topicsRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
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

            var topic = command.IsManual
                ? Topic.CreateManually(command.Key, command.Name)
                : Topic.Create(command.Key, command.Name);

            var createdTopic = await _topicsRepository.CreateTopicAsync(topic, cancellationToken);

            var topicChangedEvent = new TopicChangedEvent
            {
                TopicId = createdTopic.Id,
                Key = createdTopic.Key,
                Name = createdTopic.Name,
                IsVisible = createdTopic.IsVisible
            };
            await _eventPublisher.PublishAsync(topicChangedEvent, cancellationToken);

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
