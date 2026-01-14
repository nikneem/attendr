using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.Groups.Features.ProcessProfileCheckedIn;
using HexMaster.Attendr.Groups.Features.ProcessProfileConferenceAttendanceChanged;
using HexMaster.Attendr.Groups.Features.ProcessGroupMemberAdded;
using HexMaster.Attendr.Groups.Features.ProcessGroupMemberRemoved;
using HexMaster.Attendr.IntegrationEvents.Constants;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using HexMaster.Attendr.IntegrationEvents.Events.Groups;

namespace HexMaster.Attendr.Groups.Api.Endpoints;

/// <summary>
/// Extension methods to map Dapr event handler endpoints.
/// </summary>
public static class EventHandlersEndpoints
{
    /// <summary>
    /// Maps the event handler endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static IEndpointRouteBuilder MapEventHandlersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/EventHandlers")
            .WithName("EventHandlers");

        group.MapPost("/ProfileCheckedInHandler", HandleProfileCheckedIn)
            .WithName("HandleProfileCheckedIn")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ProfileCheckedIn)
            .Accepts<ProfileCheckedInEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        group.MapPost("/ProfileConferenceAttendanceChangedHandler", HandleProfileConferenceAttendanceChanged)
            .WithName("HandleProfileConferenceAttendanceChanged")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ProfileConferenceAttendanceChanged)
            .Accepts<ProfileConferenceAttendanceChangedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        group.MapPost("/GroupMemberAddedHandler", HandleGroupMemberAdded)
            .WithName("HandleGroupMemberAdded")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.GroupMemberAdded)
            .Accepts<GroupMemberAddedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        group.MapPost("/GroupMemberRemovedHandler", HandleGroupMemberRemoved)
            .WithName("HandleGroupMemberRemoved")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.GroupMemberRemoved)
            .Accepts<GroupMemberRemovedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> HandleProfileCheckedIn(
        ProfileCheckedInEvent @event,
        ICommandHandler<ProcessProfileCheckedInCommand> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ProfileCheckedInEventHandler");
        try
        {
            logger.LogInformation(
                "Processing ProfileCheckedIn event for profile {ProfileId}, presentation {PresentationId}, isCheckedIn: {IsCheckedIn}",
                @event.ProfileId,
                @event.PresentationId,
                @event.IsCheckedIn);

            await handler.Handle(new ProcessProfileCheckedInCommand(@event), cancellationToken);

            return Results.Ok(new
            {
                message = "Profile check-in processed and group activities updated",
                profileId = @event.ProfileId,
                presentationId = @event.PresentationId,
                isCheckedIn = @event.IsCheckedIn
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProfileCheckedIn for profile {ProfileId}, presentation {PresentationId}",
                @event.ProfileId,
                @event.PresentationId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }

    private static async Task<IResult> HandleProfileConferenceAttendanceChanged(
        ProfileConferenceAttendanceChangedEvent @event,
        ICommandHandler<ProcessProfileConferenceAttendanceChangedCommand> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ProfileConferenceAttendanceChangedEventHandler");
        try
        {
            logger.LogInformation(
                "Processing ProfileConferenceAttendanceChanged event for profile {ProfileId}, conference {ConferenceId}, isAttending: {IsAttending}",
                @event.ProfileId,
                @event.ConferenceId,
                @event.IsAttending);

            await handler.Handle(new ProcessProfileConferenceAttendanceChangedCommand(@event), cancellationToken);

            return Results.Ok(new
            {
                message = "Conference attendance processed and group activities updated",
                profileId = @event.ProfileId,
                conferenceId = @event.ConferenceId,
                isAttending = @event.IsAttending
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProfileConferenceAttendanceChanged for profile {ProfileId}, conference {ConferenceId}",
                @event.ProfileId,
                @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }

    private static async Task<IResult> HandleGroupMemberAdded(
        GroupMemberAddedEvent @event,
        ICommandHandler<ProcessGroupMemberAddedCommand> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("GroupMemberAddedEventHandler");
        try
        {
            logger.LogInformation(
                "Processing GroupMemberAdded event for profile {ProfileId} in group {GroupId}",
                @event.ProfileId,
                @event.GroupId);

            await handler.Handle(new ProcessGroupMemberAddedCommand(@event), cancellationToken);

            return Results.Ok(new
            {
                message = "Group member added activity recorded",
                profileId = @event.ProfileId,
                groupId = @event.GroupId,
                role = @event.Role
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling GroupMemberAdded for profile {ProfileId} in group {GroupId}",
                @event.ProfileId,
                @event.GroupId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }

    private static async Task<IResult> HandleGroupMemberRemoved(
        GroupMemberRemovedEvent @event,
        ICommandHandler<ProcessGroupMemberRemovedCommand> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("GroupMemberRemovedEventHandler");
        try
        {
            logger.LogInformation(
                "Processing GroupMemberRemoved event for profile {ProfileId} in group {GroupId}",
                @event.ProfileId,
                @event.GroupId);

            await handler.Handle(new ProcessGroupMemberRemovedCommand(@event), cancellationToken);

            return Results.Ok(new
            {
                message = "Group member removed activity recorded",
                profileId = @event.ProfileId,
                groupId = @event.GroupId
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling GroupMemberRemoved for profile {ProfileId} in group {GroupId}",
                @event.ProfileId,
                @event.GroupId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }
}
