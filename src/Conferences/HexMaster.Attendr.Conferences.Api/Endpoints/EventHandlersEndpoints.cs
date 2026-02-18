using Dapr;
using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Conferences;
using HexMaster.Attendr.Conferences.Abstractions.Services;
using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.IntegrationEvents.Constants;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Events.Topics;
using HexMaster.Attendr.IntegrationEvents.Models;
using HexMaster.Attendr.IntegrationEvents.Services;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class EventHandlersEndpoints
{
    public static IEndpointRouteBuilder MapEventHandlersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/EventHandlers")
            .WithName("EventHandlers");

        group.MapPost("/ConferenceCreatedHandler", ConferenceCreatedHandler)
            .WithName("ConferenceCreatedHandler")
            .AllowAnonymous()
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ConferenceCreated)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/ConferenceUpdatedHandler", ConferenceUpdatedHandler)
            .WithName("ConferenceUpdatedHandler")
            .AllowAnonymous()
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ConferenceUpdated)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/TopicChangedHandler", TopicChangedHandler)
            .WithName("TopicChangedHandler")
            .AllowAnonymous()
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.TopicChanged)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> ConferenceCreatedHandler(
        ConferenceCreatedEvent @event,
        ISessionizeSyncService sessionizeSyncService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing ConferenceCreated event for conference {ConferenceId}", @event.ConferenceId);

            var result = await sessionizeSyncService.SynchronizeConferenceAsync(@event.ConferenceId, cancellationToken);

            if (result == null)
            {
                logger.LogWarning("Conference {ConferenceId} not found", @event.ConferenceId);
                return Results.BadRequest(new { error = "Conference not found" });
            }

            return Results.Ok(new
            {
                message = "Conference synchronized successfully",
                conferenceId = result.ConferenceId,
                speakersCount = result.SpeakersCount,
                roomsCount = result.RoomsCount,
                presentationsCount = result.PresentationsCount
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing ConferenceCreated event for conference {ConferenceId}", @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to process conference created event", details = ex.Message });
        }
    }

    private static async Task<IResult> ConferenceUpdatedHandler(
        ConferenceUpdatedEvent @event,
        ISessionizeSyncService sessionizeSyncService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing ConferenceUpdated event for conference {ConferenceId}", @event.ConferenceId);

            var result = await sessionizeSyncService.SynchronizeConferenceAsync(@event.ConferenceId, cancellationToken);

            if (result == null)
            {
                logger.LogWarning("Conference {ConferenceId} not found", @event.ConferenceId);
                return Results.BadRequest(new { error = "Conference not found" });
            }

            return Results.Ok(new
            {
                message = "Conference synchronized successfully",
                conferenceId = result.ConferenceId,
                speakersCount = result.SpeakersCount,
                roomsCount = result.RoomsCount,
                presentationsCount = result.PresentationsCount
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing ConferenceUpdated event for conference {ConferenceId}", @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to process conference updated event", details = ex.Message });
        }
    }

    private static async Task<IResult> TopicChangedHandler(
        TopicChangedEvent @event,
        ITopicsRepository topicsRepository,
        IConferenceRepository conferenceRepository,
        IIntegrationEventPublisher eventPublisher,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing TopicChanged event for topic {TopicId} (key: {Key})",
                @event.TopicId, @event.Key);

            var affectedPresentations = await topicsRepository
                .GetFuturePresentationsByTopicIdAsync(@event.TopicId, cancellationToken);

            logger.LogInformation(
                "Found {Count} future presentations affected by topic {TopicId} change",
                affectedPresentations.Count, @event.TopicId);

            foreach (var (conferenceId, presentationId) in affectedPresentations)
            {
                var presentation = await conferenceRepository
                    .GetPresentationByIdAsync(conferenceId, presentationId, cancellationToken);

                if (presentation == null)
                {
                    logger.LogWarning(
                        "Presentation {PresentationId} not found in conference {ConferenceId}",
                        presentationId, conferenceId);
                    continue;
                }

                var integrationEvent = new PresentationUpdatedEvent
                {
                    ConferenceId = conferenceId,
                    PresentationId = presentationId,
                    Title = presentation.Title,
                    Abstract = presentation.Abstract,
                    StartDateTime = presentation.StartDateTime,
                    EndDateTime = presentation.EndDateTime,
                    RoomId = presentation.Room.Id,
                    RoomName = presentation.Room.Name,
                    Speakers = presentation.Speakers.Select(s => new SpeakerDto(s.Id, s.Name, s.ProfilePictureUrl)).ToList(),
                    Topics = presentation.Topics.Select(t => new PresentationTopicDto(t.Key, t.Name)).ToList(),
                    ExternalId = presentation.ExternalId,
                    IsScheduleChanged = false
                };

                await eventPublisher.PublishAsync(integrationEvent, cancellationToken);

                logger.LogInformation(
                    "Published PresentationUpdatedEvent for presentation {PresentationId} due to topic {TopicId} change",
                    presentationId, @event.TopicId);
            }

            return Results.Ok(new
            {
                message = "TopicChanged event processed successfully",
                topicId = @event.TopicId,
                affectedPresentations = affectedPresentations.Count
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing TopicChanged event for topic {TopicId}", @event.TopicId);
            return Results.BadRequest(new { error = "Failed to process topic changed event", details = ex.Message });
        }
    }
}
