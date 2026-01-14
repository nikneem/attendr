using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.IntegrationEvents.Constants;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using HexMaster.Attendr.IntegrationEvents.Events.Groups;
using HexMaster.Attendr.Presence.Features.CreateConferencePresence;
using HexMaster.Attendr.Presence.Features.UpdateConference;
using HexMaster.Attendr.Presence.Features.UpdatePresentation;

namespace HexMaster.Attendr.Presence.Api.Endpoints;

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

        group.MapPost("/ProfileFollowedConferenceHandler", HandleProfileFollowedConference)
            .WithName("HandleProfileFollowedConference")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ProfileFollowedConference)
            .Accepts<ProfileFollowedConferenceEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        group.MapPost("/ProfilesFollowedConferenceHandler", HandleProfilesFollowedConference)
            .WithName("HandleProfilesFollowedConference")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ProfilesFollowedConference)
            .Accepts<ProfilesFollowedConferenceEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        group.MapPost("/PresentationUpdatedHandler", HandlePresentationUpdated)
            .WithName("HandlePresentationUpdated")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.PresentationUpdated)
            .Accepts<PresentationUpdatedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        group.MapPost("/ConferenceUpdatedHandler", HandleConferenceUpdated)
            .WithName("HandleConferenceUpdated")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ConferenceUpdated)
            .Accepts<ConferenceUpdatedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> HandleProfileFollowedConference(
        ProfileFollowedConferenceEvent @event,
        ICommandHandler<CreateConferencePresenceCommand> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ProfileFollowedConferenceEventHandler");
        try
        {
            logger.LogInformation(
                "Processing ProfileFollowedConference event for profile {ProfileId} and conference {ConferenceId}",
                @event.ProfileId,
                @event.ConferenceId);

            await handler.Handle(
                new CreateConferencePresenceCommand(@event.ConferenceId, new[] { @event.ProfileId }),
                cancellationToken);

            return Results.Ok(new
            {
                message = "Conference presence created",
                conferenceId = @event.ConferenceId,
                profileId = @event.ProfileId
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProfileFollowedConference for profile {ProfileId} conference {ConferenceId}",
                @event.ProfileId, @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }

    private static async Task<IResult> HandleProfilesFollowedConference(
        ProfilesFollowedConferenceEvent @event,
        ICommandHandler<CreateConferencePresenceCommand> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ProfilesFollowedConferenceEventHandler");
        try
        {
            logger.LogInformation(
                "Processing ProfilesFollowedConference event for {ProfileCount} profiles and conference {ConferenceId}",
                @event.ProfileIds.Count,
                @event.ConferenceId);

            await handler.Handle(
                new CreateConferencePresenceCommand(@event.ConferenceId, @event.ProfileIds),
                cancellationToken);

            return Results.Ok(new
            {
                message = "Conference presences created",
                conferenceId = @event.ConferenceId,
                profileCount = @event.ProfileIds.Count
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProfilesFollowedConference for conference {ConferenceId}",
                @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }

    private static async Task<IResult> HandlePresentationUpdated(
        PresentationUpdatedEvent @event,
        ICommandHandler<UpdatePresentationCommand> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("PresentationUpdatedEventHandler");
        try
        {
            logger.LogInformation(
                "Processing PresentationUpdated event for conference {ConferenceId}, presentation {PresentationId}",
                @event.ConferenceId,
                @event.PresentationId);

            await handler.Handle(new UpdatePresentationCommand(@event), cancellationToken);

            return Results.Ok(new
            {
                message = "Presentation updated",
                conferenceId = @event.ConferenceId,
                presentationId = @event.PresentationId
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling PresentationUpdated for conference {ConferenceId}, presentation {PresentationId}",
                @event.ConferenceId, @event.PresentationId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }

    private static async Task<IResult> HandleConferenceUpdated(
        ConferenceUpdatedEvent @event,
        ICommandHandler<UpdateConferenceCommand> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ConferenceUpdatedEventHandler");
        try
        {
            logger.LogInformation(
                "Processing ConferenceUpdated event for conference {ConferenceId}: {Title}",
                @event.ConferenceId,
                @event.Title);

            await handler.Handle(new UpdateConferenceCommand(@event), cancellationToken);

            return Results.Ok(new
            {
                message = "Conference presence records updated",
                conferenceId = @event.ConferenceId,
                title = @event.Title
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ConferenceUpdated for conference {ConferenceId}",
                @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }
}
