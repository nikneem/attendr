using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using HexMaster.Attendr.IntegrationEvents.Services;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Constants;
using HexMaster.Attendr.Profiles.Observability;
using HexMaster.Attendr.Profiles.Repositories;
using HexMaster.Attendr.Profiles.Services;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Profiles.Features.SetTopicManualStatus;

/// <summary>
/// Command handler to set the manual status of a profile topic.
/// </summary>
public sealed class SetTopicManualStatusCommandHandler : ICommandHandler<SetTopicManualStatusCommand, ProfileTopicDto>
{
    private readonly IProfileTopicRepository _repository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TopicWeightDecayService _decayService;
    private readonly ProfileMetrics _metrics;
    private readonly ILogger<SetTopicManualStatusCommandHandler> _logger;

    public SetTopicManualStatusCommandHandler(
        IProfileTopicRepository repository,
        IIntegrationEventPublisher eventPublisher,
        TopicWeightDecayService decayService,
        ProfileMetrics metrics,
        ILogger<SetTopicManualStatusCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _decayService = decayService ?? throw new ArgumentNullException(nameof(decayService));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProfileTopicDto> Handle(SetTopicManualStatusCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.TopicId, nameof(command.TopicId));

        using var activity = ActivitySources.Profiles.StartActivity("SetTopicManualStatus", ActivityKind.Internal);
        activity?.SetTag("topic.id", command.TopicId);
        activity?.SetTag("topic.is_manual", command.IsManual);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Setting manual status for topic {TopicId} to {IsManual}", command.TopicId, command.IsManual);

            var topic = await _repository.GetByIdAsync(command.TopicId, cancellationToken);
            if (topic is null)
            {
                _metrics.RecordOperationFailed("SetTopicManualStatus", "TopicNotFound");
                throw new KeyNotFoundException($"Topic with ID '{command.TopicId}' was not found.");
            }

            topic.SetIsManual(command.IsManual);

            await _repository.UpsertAsync(topic, cancellationToken);

            // Publish ProfileTopicsChangedEvent
            var allTopics = await _repository.GetByProfileIdAsync(topic.ProfileId, cancellationToken);
            await PublishProfileTopicsChangedEventAsync(
                topic.ProfileId,
                allTopics,
                cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("SetTopicManualStatus", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Topic {TopicId} manual status set to {IsManual} successfully", command.TopicId, command.IsManual);

            // Calculate total weight using decay service for consistency
            var occasions = topic.Occasions.Select(o => (o.Weight, o.Date));
            var totalWeight = _decayService.CalculateTopicWeight(topic.IsManual, occasions);
            return new ProfileTopicDto(
                topic.Id,
                topic.ProfileId,
                topic.TopicKey,
                topic.TopicName,
                topic.IsManual,
                topic.CreatedOn,
                totalWeight);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("SetTopicManualStatus", ex.GetType().Name);
            _metrics.RecordOperationDuration("SetTopicManualStatus", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to set manual status for topic {TopicId}", command.TopicId);
            throw;
        }
    }

    private async Task PublishProfileTopicsChangedEventAsync(
        string profileId,
        IReadOnlyList<DomainModels.ProfileTopic> topics,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var maxAgeDate = now.AddMonths(-TopicWeightConstants.MaxOccasionAgeMonths);

        var topicInfos = topics
            .Select(topic =>
            {
                int totalWeight;
                if (topic.IsManual)
                {
                    totalWeight = 100;
                }
                else
                {
                    // Filter occasions within the max age timespan
                    var relevantOccasions = topic.Occasions
                        .Where(o => o.Date >= maxAgeDate)
                        .ToList();

                    // Calculate total weight with exponential decay
                    var totalDecayedWeight = relevantOccasions
                        .Sum(o => _decayService.CalculateDecayedWeight(o.Weight, o.Date, now));

                    // Cap the total weight at 100
                    totalWeight = Math.Min(totalDecayedWeight, 100);
                }

                return new ProfileTopicInfo(
                    topic.TopicKey,
                    topic.TopicName,
                    totalWeight);
            })
            .ToList();

        var @event = new ProfileTopicsChangedEvent
        {
            ProfileId = profileId,
            Topics = topicInfos
        };

        await _eventPublisher.PublishAsync(@event, cancellationToken);
    }
}
