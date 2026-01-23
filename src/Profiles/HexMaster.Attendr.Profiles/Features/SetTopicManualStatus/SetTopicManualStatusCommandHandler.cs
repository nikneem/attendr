using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Observability;
using HexMaster.Attendr.Profiles.Repositories;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Profiles.Features.SetTopicManualStatus;

/// <summary>
/// Command handler to set the manual status of a profile topic.
/// </summary>
public sealed class SetTopicManualStatusCommandHandler : ICommandHandler<SetTopicManualStatusCommand, ProfileTopicDto>
{
    private readonly IProfileTopicRepository _repository;
    private readonly ProfileMetrics _metrics;
    private readonly ILogger<SetTopicManualStatusCommandHandler> _logger;

    public SetTopicManualStatusCommandHandler(
        IProfileTopicRepository repository,
        ProfileMetrics metrics,
        ILogger<SetTopicManualStatusCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
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

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("SetTopicManualStatus", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Topic {TopicId} manual status set to {IsManual} successfully", command.TopicId, command.IsManual);

            // Manual topics always have a weight of 100
            var totalWeight = topic.IsManual ? 100 : topic.Occasions.Sum(o => o.Weight);
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
}
