using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using HexMaster.Attendr.IntegrationEvents.Services;
using HexMaster.Attendr.Profiles.Constants;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Repositories;
using HexMaster.Attendr.Profiles.Services;

namespace HexMaster.Attendr.Profiles.Api.Endpoints;

/// <summary>
/// Event handler for Dapr pub/sub topic integration events.
/// </summary>
public static class EventHandlers
{
    /// <summary>
    /// Handles ProfileTopicInterestEvent by creating or updating profile topic interest.
    /// </summary>
    public static async Task<IResult> HandleProfileTopicInterestEvent(
        ProfileTopicInterestEvent @event,
        IProfileTopicRepository repository,
        IIntegrationEventPublisher eventPublisher,
        TopicWeightDecayService decayService,
        CancellationToken cancellationToken)
    {
        try
        {
            if (@event is null)
            {
                return Results.BadRequest(new { error = "Event is required" });
            }

            if (string.IsNullOrWhiteSpace(@event.ProfileId) ||
                string.IsNullOrWhiteSpace(@event.TopicKey) ||
                string.IsNullOrWhiteSpace(@event.TopicName) ||
                @event.Weight < 0 ||
                @event.Weight > 100)
            {
                return Results.BadRequest(new { error = "Invalid event data" });
            }

            // Try to get existing topic
            var existingTopic = await repository.GetByProfileIdAndKeyAsync(
                @event.ProfileId,
                @event.TopicKey,
                cancellationToken);

            ProfileTopic topic;

            if (existingTopic is not null)
            {
                // Add occasion to existing topic
                existingTopic.AddOccasion(@event.Weight, DateTimeOffset.UtcNow);
                topic = existingTopic;
            }
            else
            {
                // Create new topic with initial occasion
                var occasion = new Occasion(@event.Weight, DateTimeOffset.UtcNow);
                topic = ProfileTopic.Create(
                    @event.ProfileId,
                    @event.TopicKey,
                    @event.TopicName,
                    @event.IsManual,
                    new[] { occasion });
            }

            // Upsert the topic (insert or update)
            await repository.UpsertAsync(topic, cancellationToken);

            // Get all topics for the profile and publish ProfileTopicsChangedEvent
            var allTopics = await repository.GetByProfileIdAsync(@event.ProfileId, cancellationToken);
            await PublishProfileTopicsChangedEventAsync(
                @event.ProfileId,
                allTopics,
                decayService,
                eventPublisher,
                cancellationToken);

            return Results.Ok(new { message = "Topic interest recorded successfully" });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task PublishProfileTopicsChangedEventAsync(
        string profileId,
        IReadOnlyList<ProfileTopic> topics,
        TopicWeightDecayService decayService,
        IIntegrationEventPublisher eventPublisher,
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
                        .Sum(o => decayService.CalculateDecayedWeight(o.Weight, o.Date, now));

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

        await eventPublisher.PublishAsync(@event, cancellationToken);
    }
}
