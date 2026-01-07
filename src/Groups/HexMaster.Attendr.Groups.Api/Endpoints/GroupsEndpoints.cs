using System.Security.Claims;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Groups.Abstractions.Dtos;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;

namespace HexMaster.Attendr.Groups.Api.Endpoints;

/// <summary>
/// Maps all group-related endpoints using a feature-sliced architecture.
/// </summary>
public static class GroupsEndpoints
{
    public static IEndpointRouteBuilder MapGroupsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/groups")
            .WithName("Groups")
            .RequireAuthorization();

        // Map group creation endpoint
        group.MapPost("/", CreateGroup)
            .WithName("CreateGroup")
            .Produces<CreateGroupResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Map query endpoints (list, get details, my groups)
        group.MapGroupsQueryEndpoints();

        // Map member management endpoints (join, remove, approve, deny)
        group.MapGroupsMemberEndpoints();

        // Map conference-related endpoints
        var conferencesGroup = group.MapGroup("/{id:guid}/conferences")
            .WithName("GroupConferences");
        conferencesGroup.MapGroupsConferenceEndpoints();

        return app;
    }

    private static async Task<IResult> CreateGroup(
        CreateGroupRequest request,
        IProfilesIntegrationService profilesIntegration,
        IGroupRepository repository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest(new { error = "Name is required" });
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(request.Name, @"^[a-zA-Z0-9\s]+$"))
        {
            return Results.BadRequest(new { error = "Name can only contain alphanumeric characters and spaces" });
        }

        if (request.Name.Trim().Length < 3)
        {
            return Results.BadRequest(new { error = "Name must be at least 3 characters long" });
        }

        if (request.Name.Trim().Length > 100)
        {
            return Results.BadRequest(new { error = "Name must not exceed 100 characters" });
        }

        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            var group = Group.Create(
                request.Name.Trim(),
                Guid.Parse(profile.ProfileId),
                profile.DisplayName);

            await repository.AddAsync(group, cancellationToken);

            var memberDtos = group.Members.Select(m => new GroupMemberDto(m.Id, m.Name, (Abstractions.Dtos.GroupRole)m.Role)).ToList();
            var result = new CreateGroupResult(group.Id, group.Name, memberDtos);

            return Results.Created($"/api/groups/{result.Id}", result);
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
