using System.Security.Claims;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Groups.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Integrations.Services;

namespace HexMaster.Attendr.Groups.Api.Endpoints;

/// <summary>
/// Endpoints for conference-related operations within groups.
/// </summary>
public static class GroupsConferenceEndpoints
{
    public static RouteGroupBuilder MapGroupsConferenceEndpoints(this RouteGroupBuilder conferencesGroup)
    {
        conferencesGroup.MapPost("/", FollowConference)
            .WithName("FollowConference")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        conferencesGroup.MapDelete("/{conferenceId:guid}", UnfollowConference)
            .WithName("UnfollowConference")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        conferencesGroup.MapGet("/", GetFollowedConferences)
            .WithName("GetFollowedConferences")
            .Produces<IReadOnlyCollection<FollowedConferenceDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return conferencesGroup;
    }

    private static async Task<IResult> FollowConference(
        Guid id,
        FollowConferenceRequest request,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<Features.FollowConference.FollowConferenceCommand> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (request.ConferenceId == Guid.Empty)
        {
            return Results.BadRequest(new { error = "Conference ID is required" });
        }

        var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return Results.Unauthorized();
        }

        var profile = await profilesIntegration.ResolveProfile(subjectId, cancellationToken);
        if (profile is null)
        {
            return Results.NotFound(new { error = "User profile not found. Please create a profile first." });
        }

        var command = new Features.FollowConference.FollowConferenceCommand(id, Guid.Parse(profile.ProfileId), request.ConferenceId);
        await handler.Handle(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> UnfollowConference(
        Guid id,
        Guid conferenceId,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<Features.UnfollowConference.UnfollowConferenceCommand> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return Results.Unauthorized();
        }

        var profile = await profilesIntegration.ResolveProfile(subjectId, cancellationToken);
        if (profile is null)
        {
            return Results.NotFound(new { error = "User profile not found. Please create a profile first." });
        }

        var command = new Features.UnfollowConference.UnfollowConferenceCommand(id, Guid.Parse(profile.ProfileId), conferenceId);
        await handler.Handle(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetFollowedConferences(
        Guid id,
        IProfilesIntegrationService profilesIntegration,
        IQueryHandler<Features.GetGroupFollowedConferences.GetGroupFollowedConferencesQuery, IReadOnlyCollection<FollowedConferenceDto>> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return Results.Unauthorized();
        }

        var profile = await profilesIntegration.ResolveProfile(subjectId, cancellationToken);
        if (profile is null)
        {
            return Results.NotFound(new { error = "User profile not found. Please create a profile first." });
        }

        var query = new Features.GetGroupFollowedConferences.GetGroupFollowedConferencesQuery(id, Guid.Parse(profile.ProfileId));
        var conferences = await handler.Handle(query, cancellationToken);

        return Results.Ok(conferences);
    }
}
