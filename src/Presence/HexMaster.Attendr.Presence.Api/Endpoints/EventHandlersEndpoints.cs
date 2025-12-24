using Dapr;
using Dapr.Client;
using HexMaster.Attendr.IntegrationEvents.Events;
using HexMaster.Attendr.Presence.Api.Services;
using HexMaster.Attendr.Presence.Services;
using System.Security.Claims;

namespace HexMaster.Attendr.Presence.Api.Endpoints;

public static class PresenceEndpoints
{
    public static IEndpointRouteBuilder MapPresenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/presence")
            .WithName("Presence")
            .RequireAuthorization();

        group.MapGet("/my-conferences", GetMyConferences)
            .WithName("GetMyConferences")
            .Produces<List<MyConferenceResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> GetMyConferences(
        HttpContext context,
        IConferencePresenceRepository repository,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        var profileIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (profileIdClaim is null || !Guid.TryParse(profileIdClaim.Value, out var profileId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var allPresences = await repository.GetByProfileIdAsync(profileId, cancellationToken);
            var now = DateTime.UtcNow;

            var currentAndFuture = allPresences
                .Where(p => p.EndDate >= DateOnly.FromDateTime(now))
                .OrderBy(p => p.StartDate)
                .Select(p => new MyConferenceResponse(
                    p.ConferenceId,
                    p.ConferenceName,
                    p.Location,
                    p.StartDate.ToDateTime(TimeOnly.MinValue),
                    p.EndDate.ToDateTime(TimeOnly.MaxValue),
                    p.IsAttending))
                .ToList();

            return Results.Ok(currentAndFuture);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving conferences for profile {ProfileId}", profileId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

public sealed record MyConferenceResponse(
    Guid ConferenceId,
    string ConferenceName,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAttending);

public static class EventHandlers
{
    public static IEndpointRouteBuilder MapEventHandlers(this IEndpointRouteBuilder app)
    {
        app.MapPost("/events/profile-followed-conference",
            ProfileFollowedConferenceHandler)
            .WithName("HandleProfileFollowedConference")
            .Accepts<ProfileFollowedConferenceEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPost("/events/profiles-followed-conference",
            ProfilesFollowedConferenceHandler)
            .WithName("HandleProfilesFollowedConference")
            .Accepts<ProfilesFollowedConferenceEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }

    [Topic("dapr-pubsub", "profile-followed-conference")]
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

    [Topic("dapr-pubsub", "profiles-followed-conference")]
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
}
