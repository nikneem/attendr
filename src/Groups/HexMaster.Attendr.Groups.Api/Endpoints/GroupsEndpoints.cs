using System.Security.Claims;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.Groups.Abstractions.Dtos;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.GetMyGroups;
using HexMaster.Attendr.Groups.ListGroups;
using HexMaster.Attendr.Profiles.Integrations.Services;

namespace HexMaster.Attendr.Groups.Api.Endpoints;

public static class GroupsEndpoints
{
    public static IEndpointRouteBuilder MapGroupsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/groups")
            .WithName("Groups");

        group.MapPost("/", CreateGroup)
            .WithName("CreateGroup")
            .Produces<CreateGroupResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapGet("/my-groups", GetMyGroups)
            .WithName("GetMyGroups")
            .Produces<IReadOnlyCollection<MyGroupDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapGet("/", ListGroups)
            .WithName("ListGroups")
            .Produces<ListGroupsResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> CreateGroup(
        CreateGroupRequest request,
        IProfilesIntegrationService profilesIntegration,
        IGroupRepository repository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest(new { error = "Name is required" });
        }

        // Extract SubjectId from JWT token
        var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return Results.Unauthorized();
        }

        try
        {
            // Resolve the current user's profile
            var profile = await profilesIntegration.ResolveProfile(subjectId, cancellationToken);
            if (profile is null)
            {
                return Results.NotFound(new { error = "User profile not found. Please create a profile first." });
            }

            // Create the group with the current user as owner
            var group = Group.Create(
                request.Name.Trim(),
                Guid.Parse(profile.ProfileId),
                profile.DisplayName);

            // Persist the group
            await repository.AddAsync(group, cancellationToken);

            // Return created group details
            var memberDtos = group.Members.Select(m => new GroupMemberDto(m.Id, m.Name, m.Role)).ToList();
            var result = new CreateGroupResult(group.Id, group.Name, memberDtos);

            return Results.Created($"/api/groups/{result.Id}", result);
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetMyGroups(
        IProfilesIntegrationService profilesIntegration,
        IQueryHandler<GetMyGroupsQuery, IReadOnlyCollection<MyGroupDto>> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // Extract SubjectId from JWT token
        var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return Results.Unauthorized();
        }

        try
        {
            // Resolve the current user's profile
            var profile = await profilesIntegration.ResolveProfile(subjectId, cancellationToken);
            if (profile is null)
            {
                return Results.NotFound(new { error = "User profile not found. Please create a profile first." });
            }

            // Get user's groups
            var query = new GetMyGroupsQuery(Guid.Parse(profile.ProfileId));
            var groups = await handler.Handle(query, cancellationToken);

            return Results.Ok(groups);
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
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
        // Extract SubjectId from JWT token
        var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return Results.Unauthorized();
        }

        try
        {
            // Resolve the current user's profile
            var profile = await profilesIntegration.ResolveProfile(subjectId, cancellationToken);
            if (profile is null)
            {
                return Results.NotFound(new { error = "User profile not found. Please create a profile first." });
            }

            // Normalize pagination parameters
            var normalizedPageSize = PaginationConstants.NormalizePageSize(pageSize);
            var normalizedPageNumber = Math.Max(1, pageNumber ?? 1);

            // Create and execute query
            var query = new ListGroupsQuery(
                Guid.Parse(profile.ProfileId),
                searchQuery,
                normalizedPageSize,
                normalizedPageNumber);

            var result = await handler.Handle(query, cancellationToken);

            return Results.Ok(result);
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
