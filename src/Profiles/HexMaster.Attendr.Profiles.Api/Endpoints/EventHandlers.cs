using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
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

    /// <summary>
    /// Handles ConferencePresentationsImportedEvent by publishing ProfileTopicsChangedEvent for each affected profile.
    /// </summary>
    public static async Task<IResult> HandleConferencePresentationsImportedEvent(
        ConferencePresentationsImportedEvent @event,
        IProfileTopicRepository repository,
        IIntegrationEventPublisher eventPublisher,
        TopicWeightDecayService decayService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            if (@event is null)
            {
                return Results.BadRequest(new { error = "Event is required" });
            }

            logger.LogInformation(
                "Processing ConferencePresentationsImported event for conference {ConferenceId} with {ProfileCount} profiles and {PresentationsCount} presentations",
                @event.ConferenceId,
                @event.ProfileIds.Count,
                @event.PresentationsCount);

            var processedCount = 0;

            // Process each profile that follows this conference
            foreach (var profileId in @event.ProfileIds)
            {
                // Get all topics for the profile
                var topics = await repository.GetByProfileIdAsync(profileId.ToString(), cancellationToken);

                // Only publish if profile has topics
                if (topics.Count > 0)
                {
                    await PublishProfileTopicsChangedEventAsync(
                        profileId.ToString(),
                        topics,
                        decayService,
                        eventPublisher,
                        cancellationToken);

                    processedCount++;

                    logger.LogDebug(
                        "Published ProfileTopicsChangedEvent for profile {ProfileId} with {TopicCount} topics",
                        profileId,
                        topics.Count);
                }
                else
                {
                    logger.LogDebug(
                        "Skipping profile {ProfileId} - no topics found",
                        profileId);
                }
            }

            logger.LogInformation(
                "Successfully processed ConferencePresentationsImported event for conference {ConferenceId} ({ConferenceName}). Published ProfileTopicsChangedEvent for {ProcessedCount}/{TotalCount} profiles",
                @event.ConferenceId,
                @event.ConferenceName,
                processedCount,
                @event.ProfileIds.Count);

            return Results.Ok(new
            {
                message = "Conference presentations imported event processed",
                conferenceId = @event.ConferenceId,
                conferenceName = @event.ConferenceName,
                profilesProcessed = processedCount,
                totalProfiles = @event.ProfileIds.Count
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ConferencePresentationsImported for conference {ConferenceId}",
                @event.ConferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
