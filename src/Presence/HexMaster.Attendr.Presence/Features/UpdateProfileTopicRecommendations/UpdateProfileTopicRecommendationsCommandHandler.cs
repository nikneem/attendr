using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.UpdateProfileTopicRecommendations;

/// <summary>
/// Handles updating presentation recommendations based on profile topic changes.
/// </summary>
public sealed class UpdateProfileTopicRecommendationsCommandHandler : ICommandHandler<UpdateProfileTopicRecommendationsCommand, int>
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<UpdateProfileTopicRecommendationsCommandHandler> _logger;

    public UpdateProfileTopicRecommendationsCommandHandler(
        IPresentationPresenceRepository repository,
        PresenceMetrics metrics,
        ILogger<UpdateProfileTopicRecommendationsCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> Handle(UpdateProfileTopicRecommendationsCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var @event = command.Event;

        using var activity = ActivitySources.Presence.StartActivity("UpdateProfileTopicRecommendations", ActivityKind.Internal);
        activity?.SetTag("profile.id", @event.ProfileId);
        activity?.SetTag("topics.count", @event.Topics.Count);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Updating presentation recommendations for profile {ProfileId} with {TopicCount} topics",
                @event.ProfileId,
                @event.Topics.Count);

            // Parse profileId from string to Guid
            if (!Guid.TryParse(@event.ProfileId, out var profileId))
            {
                _logger.LogWarning("Invalid profile ID format: {ProfileId}", @event.ProfileId);
                throw new ArgumentException($"Invalid profile ID format: {@event.ProfileId}", nameof(@event.ProfileId));
            }

            // Filter topics with weight >= 70
            var highWeightTopics = @event.Topics
                .Where(t => t.Weight >= 70)
                .Select(t => t.TopicKey)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (highWeightTopics.Count == 0)
            {
                _logger.LogInformation("No topics with weight >= 70 found for profile {ProfileId}", profileId);
                activity?.SetStatus(ActivityStatusCode.Ok);
                _metrics.RecordOperationDuration("UpdateProfileTopicRecommendations", stopwatch.Elapsed.TotalMilliseconds, success: true);
                return 0;
            }

            activity?.SetTag("topics.high_weight_count", highWeightTopics.Count);

            // Get all presentations for the profile
            var presentations = await _repository.GetByProfileAsync(profileId, cancellationToken);

            var now = DateTime.UtcNow;
            var updatedCount = 0;

            foreach (var presentation in presentations)
            {
                // Only consider future presentations
                if (presentation.StartDateTime <= now)
                {
                    continue;
                }

                // Check if any presentation topic matches high-weight profile topics
                var hasMatchingTopic = presentation.Topics
                    .Any(t => highWeightTopics.Contains(t.Key));

                // Update recommendation status if needed
                if (hasMatchingTopic && !presentation.IsRecommended)
                {
                    presentation.SetRecommended(true);
                    await _repository.UpdateAsync(
                        presentation.ProfileId,
                        presentation.ConferenceId,
                        presentation,
                        cancellationToken);
                    updatedCount++;
                }
                else if (!hasMatchingTopic && presentation.IsRecommended)
                {
                    // Also unset recommendation if topics no longer match
                    presentation.SetRecommended(false);
                    await _repository.UpdateAsync(
                        presentation.ProfileId,
                        presentation.ConferenceId,
                        presentation,
                        cancellationToken);
                    updatedCount++;
                }
            }

            activity?.SetTag("presentations.updated_count", updatedCount);
            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("UpdateProfileTopicRecommendations", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation(
                "Updated {UpdatedCount} presentation recommendations for profile {ProfileId}",
                updatedCount,
                profileId);

            return updatedCount;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("UpdateProfileTopicRecommendations", ex.GetType().Name);
            _metrics.RecordOperationDuration("UpdateProfileTopicRecommendations", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to update presentation recommendations for profile {ProfileId}", @event.ProfileId);
            throw;
        }
    }
}
