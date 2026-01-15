using Dapr;
using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.IntegrationEvents.Constants;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Events.Groups;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using HexMaster.Attendr.Notifications.Constants;
using HexMaster.Attendr.Notifications.Features.ProcessNotificationTrigger;
using Microsoft.AspNetCore.Mvc;

namespace HexMaster.Attendr.Notifications.Api.Endpoints;

/// <summary>
/// Extension methods to map Dapr event handler endpoints.
/// </summary>
public static class EventHandlersEndpoints
{
    /// <summary>
    /// Maps the event handler endpoints to the application.
    /// </summary>
    public static IEndpointRouteBuilder MapEventHandlersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/EventHandlers")
            .WithName("EventHandlers");

        // Group events
        group.MapPost("/GroupMemberAddedHandler", HandleGroupMemberAdded)
            .WithName("HandleGroupMemberAdded")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.GroupMemberAdded)
            .Accepts<GroupMemberAddedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .AllowAnonymous();

        group.MapPost("/GroupMemberRemovedHandler", HandleGroupMemberRemoved)
            .WithName("HandleGroupMemberRemoved")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.GroupMemberRemoved)
            .Accepts<GroupMemberRemovedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Profile events
        group.MapPost("/ProfileFollowedConferenceHandler", HandleProfileFollowedConference)
            .WithName("HandleProfileFollowedConference")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ProfileFollowedConference)
            .Accepts<ProfileFollowedConferenceEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Presentation events
        group.MapPost("/PresentationScheduleChangedHandler", HandlePresentationScheduleChanged)
            .WithName("HandlePresentationScheduleChanged")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.PresentationScheduleChanged)
            .Accepts<PresentationScheduleChangedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> HandleGroupMemberAdded(
        [FromBody] GroupMemberAddedEvent @event,
        ICommandHandler<ProcessNotificationTriggerCommand> handler,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing GroupMemberAdded event for group {GroupId}", @event.GroupId);

            // Notify all existing members about the new member (excluding the new member)
            // In a real scenario, you'd fetch all group members and create notifications for each
            // For now, this is a placeholder showing the pattern

            var command = new ProcessNotificationTriggerCommand(
                ProfileId: @event.ProfileId, // This would be each existing member
                TypeKey: NotificationTypeKeys.GroupMemberAdded,
                Title: "New Group Member",
                Message: $"A new member joined your group",
                StackKey: $"group:{@event.GroupId}:member-added"
            );

            await handler.Handle(command, cancellationToken);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process GroupMemberAdded event");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> HandleGroupMemberRemoved(
        [FromBody] GroupMemberRemovedEvent @event,
        ICommandHandler<ProcessNotificationTriggerCommand> handler,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing GroupMemberRemoved event for group {GroupId}", @event.GroupId);

            var command = new ProcessNotificationTriggerCommand(
                ProfileId: @event.ProfileId,
                TypeKey: NotificationTypeKeys.GroupMemberRemoved,
                Title: "Group Member Left",
                Message: $"A member left your group",
                StackKey: $"group:{@event.GroupId}:member-removed"
            );

            await handler.Handle(command, cancellationToken);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process GroupMemberRemoved event");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> HandleProfileFollowedConference(
        [FromBody] ProfileFollowedConferenceEvent @event,
        ICommandHandler<ProcessNotificationTriggerCommand> handler,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing ProfileFollowedConference event for profile {ProfileId} and conference {ConferenceId}",
                @event.ProfileId, @event.ConferenceId);

            var command = new ProcessNotificationTriggerCommand(
                ProfileId: @event.ProfileId,
                TypeKey: NotificationTypeKeys.ProfileFollowedConference,
                Title: "Following Conference",
                Message: $"You are now following the conference",
                Url: $"/conferences/{@event.ConferenceId}",
                EntityRefs: new Dictionary<string, string>
                {
                    ["conferenceId"] = @event.ConferenceId.ToString()
                }
            );

            await handler.Handle(command, cancellationToken);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process ProfileFollowedConference event");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> HandlePresentationScheduleChanged(
        [FromBody] PresentationScheduleChangedEvent @event,
        ICommandHandler<ProcessNotificationTriggerCommand> handler,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing PresentationScheduleChanged event for presentation {PresentationId}",
                @event.PresentationId);

            // For each profile that favorited this presentation, send a notification
            // In a real scenario, you'd fetch all profiles who favorited and loop
            foreach (var profileId in @event.AffectedProfileIds)
            {
                var command = new ProcessNotificationTriggerCommand(
                    ProfileId: profileId,
                    TypeKey: NotificationTypeKeys.PresentationScheduleChanged,
                    Title: "Schedule Changed",
                    Message: $"The schedule for '{@event.Title}' has changed",
                    Url: $"/conferences/{@event.ConferenceId}/presentations/{@event.PresentationId}",
                    EntityRefs: new Dictionary<string, string>
                    {
                        ["conferenceId"] = @event.ConferenceId.ToString(),
                        ["presentationId"] = @event.PresentationId.ToString()
                    }
                );

                await handler.Handle(command, cancellationToken);
            }

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process PresentationScheduleChanged event");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
