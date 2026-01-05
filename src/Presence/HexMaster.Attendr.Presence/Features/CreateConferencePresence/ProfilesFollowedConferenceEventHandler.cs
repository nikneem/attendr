using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.Presence.Features.CreateConferencePresence;

public static class ProfilesFollowedConferenceEventHandler
{
    public static IEndpointRouteBuilder MapProfilesFollowedConferenceEventHandler(this IEndpointRouteBuilder app)
    {
        app.MapPost("/events/profiles-followed-conference", HandleAsync)
            .WithName("HandleProfilesFollowedConference")
            .WithTopic(DaprConstants.PubSub.DaprPubSubName, DaprConstants.Topics.ProfilesFollowedConference)
            .Accepts<ProfilesFollowedConferenceEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ProfilesFollowedConferenceEvent @event,
        CreateConferencePresenceService service,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            await service.ExecuteAsync(
                @event.ConferenceId,
                @event.ProfileIds,
                cancellationToken);

            return Results.Ok(new { message = "Conference presences processed", conferenceId = @event.ConferenceId, profiles = @event.ProfileIds.Count });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProfilesFollowedConference for conference {ConferenceId}", @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }
}


