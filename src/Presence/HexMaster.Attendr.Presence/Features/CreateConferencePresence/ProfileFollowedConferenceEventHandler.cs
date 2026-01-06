using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.Presence.Features.CreateConferencePresence;

public static class ProfileFollowedConferenceEventHandler
{
    public static IEndpointRouteBuilder MapProfileFollowedConferenceEventHandler(this IEndpointRouteBuilder app)
    {
        app.MapPost("/events/profile-followed-conference", HandleAsync)
            .WithName("HandleProfileFollowedConference")
            .WithTopic(DaprConstants.PubSub.DaprPubSubName, DaprConstants.Topics.ProfileFollowedConference)
            .Accepts<ProfileFollowedConferenceEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ProfileFollowedConferenceEvent @event,
        ICommandHandler<CreateConferencePresenceCommand> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ProfileFollowedConferenceEventHandler");
        try
        {
            await handler.Handle(
                new CreateConferencePresenceCommand(@event.ConferenceId, new[] { @event.ProfileId }),
                cancellationToken);

            return Results.Ok(new { message = "Conference presence created", conferenceId = @event.ConferenceId, profileId = @event.ProfileId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProfileFollowedConference for profile {ProfileId} conference {ConferenceId}", @event.ProfileId, @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }
}


