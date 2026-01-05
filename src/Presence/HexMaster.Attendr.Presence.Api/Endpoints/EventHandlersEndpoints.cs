using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.IntegrationEvents.Events;
using HexMaster.Attendr.Presence.Api.Services;

namespace HexMaster.Attendr.Presence.Api.Endpoints;

/// <summary>
/// Event handler endpoints for processing integration events via Dapr pub/sub.
/// These endpoints allow anonymous access for internal event processing.
/// </summary>
public static class EventHandlersEndpoints
{
    /// <summary>
    /// Maps event handler endpoints to the application.
    /// These endpoints allow anonymous access.
    /// </summary>
    public static IEndpointRouteBuilder MapEventHandlersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/events/profile-followed-conference",
            ProfileFollowedConferenceHandler)
            .WithName("HandleProfileFollowedConference")
            .WithTopic(DaprConstants.PubSub.DaprPubSubName, DaprConstants.Topics.ProfileFollowedConference)
            .Accepts<ProfileFollowedConferenceEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        app.MapPost("/events/profiles-followed-conference",
            ProfilesFollowedConferenceHandler)
            .WithName("HandleProfilesFollowedConference")
            .WithTopic(DaprConstants.PubSub.DaprPubSubName, DaprConstants.Topics.ProfilesFollowedConference)
            .Accepts<ProfilesFollowedConferenceEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        app.MapPost("/events/presentation-updated",
            PresentationUpdatedHandler)
            .WithName("HandlePresentationUpdated")
            .WithTopic(DaprConstants.PubSub.DaprPubSubName, DaprConstants.Topics.PresentationUpdated)
            .Accepts<PresentationUpdatedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> ProfileFollowedConferenceHandler(
        ProfileFollowedConferenceEvent @event,
        ICreateConferencePresenceService createPresenceService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            await createPresenceService.CreateForProfilesAsync(
                @event.ConferenceId,
                new[] { @event.ProfileId },
                cancellationToken);

            return Results.Ok(new { message = "Conference presence created", conferenceId = @event.ConferenceId, profileId = @event.ProfileId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProfileFollowedConference for profile {ProfileId} conference {ConferenceId}", @event.ProfileId, @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }

    private static async Task<IResult> ProfilesFollowedConferenceHandler(
        ProfilesFollowedConferenceEvent @event,
        ICreateConferencePresenceService createPresenceService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            await createPresenceService.CreateForProfilesAsync(
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

    private static async Task<IResult> PresentationUpdatedHandler(
        PresentationUpdatedEvent @event,
        IUpdatePresentationService updatePresentationService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            await updatePresentationService.HandlePresentationUpdatedAsync(@event, cancellationToken);

            return Results.Ok(new { message = "Presentation updated", conferenceId = @event.ConferenceId, presentationId = @event.PresentationId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling PresentationUpdated for conference {ConferenceId}, presentation {PresentationId}", @event.ConferenceId, @event.PresentationId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }
}
