using System.Security.Claims;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;

namespace HexMaster.Attendr.Groups.Api.Endpoints;

/// <summary>
/// Endpoints for group member management operations.
/// </summary>
public static class GroupsMemberEndpoints
{
    public static RouteGroupBuilder MapGroupsMemberEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/members", JoinGroup)
            .WithName("JoinGroup")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/members/{memberId:guid}", RemoveMember)
            .WithName("RemoveMember")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/join-requests/{profileId:guid}/approve", ApproveJoinRequest)
            .WithName("ApproveJoinRequest")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/join-requests/{profileId:guid}/deny", DenyJoinRequest)
            .WithName("DenyJoinRequest")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> JoinGroup(
        Guid id,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<Features.JoinGroup.JoinGroupCommand> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            var command = new Features.JoinGroup.JoinGroupCommand(id, Guid.Parse(profile.ProfileId), profile.DisplayName);
            await handler.Handle(command, cancellationToken);

            return Results.NoContent();
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> RemoveMember(
        Guid id,
        Guid memberId,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<Features.RemoveMember.RemoveMemberCommand> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            var command = new Features.RemoveMember.RemoveMemberCommand(id, memberId, Guid.Parse(profile.ProfileId));
            await handler.Handle(command, cancellationToken);

            return Results.NoContent();
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ApproveJoinRequest(
        Guid id,
        Guid profileId,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<Features.ApproveJoinRequest.ApproveJoinRequestCommand> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            var command = new Features.ApproveJoinRequest.ApproveJoinRequestCommand(id, profileId, Guid.Parse(profile.ProfileId));
            await handler.Handle(command, cancellationToken);

            return Results.NoContent();
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DenyJoinRequest(
        Guid id,
        Guid profileId,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<Features.DenyJoinRequest.DenyJoinRequestCommand> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            var command = new Features.DenyJoinRequest.DenyJoinRequestCommand(id, profileId, Guid.Parse(profile.ProfileId));
            await handler.Handle(command, cancellationToken);

            return Results.NoContent();
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
}
