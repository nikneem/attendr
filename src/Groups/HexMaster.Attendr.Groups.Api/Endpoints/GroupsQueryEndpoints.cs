using System.Security.Claims;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Groups.Abstractions.Dtos;
using HexMaster.Attendr.Groups.Features.GetGroupDetails;
using HexMaster.Attendr.Groups.Features.GetMyGroups;
using HexMaster.Attendr.Groups.Features.ListGroups;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;

namespace HexMaster.Attendr.Groups.Api.Endpoints;

/// <summary>
/// Endpoints for querying groups.
/// </summary>
public static class GroupsQueryEndpoints
{
    public static RouteGroupBuilder MapGroupsQueryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/my-groups", GetMyGroups)
            .WithName("GetMyGroups")
            .Produces<IReadOnlyCollection<MyGroupDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", ListGroups)
            .WithName("ListGroups")
            .Produces<ListGroupsResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapGet("/{id:guid}", GetGroupDetails)
            .WithName("GetGroupDetails")
            .Produces<GroupDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetMyGroups(
        IProfilesIntegrationService profilesIntegration,
        IQueryHandler<GetMyGroupsQuery, IReadOnlyCollection<MyGroupDto>> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            var query = new GetMyGroupsQuery(Guid.Parse(profile.ProfileId));
            var groups = await handler.Handle(query, cancellationToken);

            return Results.Ok(groups);
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

    private static async Task<IResult> ListGroups(
        IProfilesIntegrationService profilesIntegration,
        IQueryHandler<ListGroupsQuery, ListGroupsResult> handler,
        ClaimsPrincipal user,
        string? searchQuery,
        int? pageSize,
        int? pageNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            //var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            var normalizedPageSize = Math.Max(1, Math.Min(100, pageSize ?? 20));
            var normalizedPageNumber = Math.Max(1, pageNumber ?? 1);

            var query = new ListGroupsQuery(
                Guid.NewGuid(),
                //Guid.Parse(profile.ProfileId),
                searchQuery ?? string.Empty,
                normalizedPageSize,
                normalizedPageNumber);

            var result = await handler.Handle(query, cancellationToken);
            return Results.Ok(result);
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

    private static async Task<IResult> GetGroupDetails(
        Guid id,
        IProfilesIntegrationService profilesIntegration,
        IQueryHandler<GetGroupDetailsQuery, GroupDetailsDto?> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            var query = new GetGroupDetailsQuery(id, Guid.Parse(profile.ProfileId));
            var group = await handler.Handle(query, cancellationToken);

            return group is null ? Results.NotFound() : Results.Ok(group);
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
