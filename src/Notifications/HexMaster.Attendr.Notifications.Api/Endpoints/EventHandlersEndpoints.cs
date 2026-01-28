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

        group.MapPost("/GroupAccessRequestedHandler", HandleGroupAccessRequested)
            .WithName("HandleGroupAccessRequested")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.GroupAccessRequested)
            .Accepts<GroupAccessRequestedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Profile events
        group.MapPost("/ProfileFollowedConferenceHandler", HandleProfileFollowedConference)
            .WithName("HandleProfileFollowedConference")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ProfileFollowedConference)
            .Accepts<ProfileFollowedConferenceEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Conference import events
        group.MapPost("/ConferencePresentationsImportedHandler", HandleConferencePresentationsImported)
            .WithName("HandleConferencePresentationsImported")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ConferencePresentationsImported)
            .Accepts<ConferencePresentationsImportedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Presentation events
        group.MapPost("/PresentationScheduleChangedHandler", HandlePresentationScheduleChanged)
            .WithName("HandlePresentationScheduleChanged")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.PresentationScheduleChanged)
            .Accepts<PresentationScheduleChangeEvent>("application/cloudevents+json")
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

    private static async Task<IResult> HandleGroupAccessRequested(
        [FromBody] GroupAccessRequestedEvent @event,
        ICommandHandler<ProcessNotificationTriggerCommand> handler,
        ILogger<Program> logger,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        try
        {

            var frontendUrl = configuration.GetValue<string>("Frontend:BaseUrl") ?? "https://attendr.live";

            logger.LogInformation(
                "Processing GroupAccessRequested event for group {GroupId} from profile {ProfileId}",
                @event.GroupId, @event.ProfileId);

            // Notify all group owners and administrators about the access request
            foreach (var target in @event.NotificationTargets)
            {
                var command = new ProcessNotificationTriggerCommand(
                    ProfileId: target.ProfileId,
                    TypeKey: NotificationTypeKeys.GroupAccessRequested,
                    Title: "Group Access Request",
                    Message: $"{@event.ProfileName} has requested to join {@event.GroupName}",
                    Url: $"{frontendUrl}/app/groups/{@event.GroupId}",
                    ActorId: @event.ProfileId,
                    EntityRefs: new Dictionary<string, string>
                    {
                        ["groupId"] = @event.GroupId.ToString(),
                        ["requesterId"] = @event.ProfileId.ToString()
                    },
                    StackKey: $"group:{@event.GroupId}:access-requested:{@event.ProfileId}"
                );

                await handler.Handle(command, cancellationToken);

                logger.LogInformation(
                    "Created notification for {TargetProfileId} about access request from {RequesterId} to group {GroupId}",
                    target.ProfileId, @event.ProfileId, @event.GroupId);
            }

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to process GroupAccessRequested event for group {GroupId}",
                @event.GroupId);
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

    private static async Task<IResult> HandleConferencePresentationsImported(
        [FromBody] ConferencePresentationsImportedEvent @event,
        ICommandHandler<ProcessNotificationTriggerCommand> handler,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing ConferencePresentationsImported event for conference {ConferenceId} with {ProfileCount} profiles and {PresentationsCount} presentations",
                @event.ConferenceId,
                @event.ProfileIds.Count,
                @event.PresentationsCount);

            // Send notification to all profiles following this conference
            foreach (var profileId in @event.ProfileIds)
            {
                var command = new ProcessNotificationTriggerCommand(
                    ProfileId: profileId,
                    TypeKey: NotificationTypeKeys.ConferencePresentationsImported,
                    Title: "Conference Imported",
                    Message: $"Conference '{@event.ConferenceName}' was successfully imported with {@event.PresentationsCount} scheduled presentations",
                    Url: $"/conferences/{@event.ConferenceId}",
                    EntityRefs: new Dictionary<string, string>
                    {
                        ["conferenceId"] = @event.ConferenceId.ToString(),
                        ["presentationsCount"] = @event.PresentationsCount.ToString()
                    }
                );

                await handler.Handle(command, cancellationToken);

                logger.LogDebug(
                    "Sent notification to profile {ProfileId} for conference import {ConferenceId}",
                    profileId,
                    @event.ConferenceId);
            }

            logger.LogInformation(
                "Successfully sent {NotificationCount} notifications for conference {ConferenceId} import",
                @event.ProfileIds.Count,
                @event.ConferenceId);

            return Results.Ok(new
            {
                message = "Conference presentations imported notifications sent",
                conferenceId = @event.ConferenceId,
                notificationsSent = @event.ProfileIds.Count
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process ConferencePresentationsImported event for conference {ConferenceId}", @event.ConferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> HandlePresentationScheduleChanged(
        [FromBody] PresentationScheduleChangeEvent @event,
        ICommandHandler<ProcessNotificationTriggerCommand> handler,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing PresentationScheduleChanged event for presentation {PresentationId}",
                @event.PresentationId);

            // Send notification to the profile who favorited this presentation
            var command = new ProcessNotificationTriggerCommand(
                ProfileId: @event.ProfileId,
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

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process PresentationScheduleChanged event");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
