using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Api.Authorization;
using HexMaster.Attendr.Conferences.Features.CreateConference;
using HexMaster.Attendr.Conferences.Features.DeleteConference;
using HexMaster.Attendr.Conferences.Features.FollowConference;
using HexMaster.Attendr.Conferences.Features.GetConference;
using HexMaster.Attendr.Conferences.Features.ListConferences;
using HexMaster.Attendr.Conferences.Features.UpdateConference;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using System.Security.Claims;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class ConferencesEndpoints
{
    public static IEndpointRouteBuilder MapConferencesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences")
            .WithName("Conferences");

        group.MapGet("/", ListConferences)
            .WithName("ListConferences")
            .Produces<ListConferencesResult>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group.MapGet("/{id:guid}", GetConference)
            .WithName("GetConference")
            .Produces<ConferenceDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapPost("/", CreateConference)
            .WithName("CreateConference")
            .Produces<CreateConferenceResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPut("/{id:guid}", UpdateConference)
            .WithName("UpdateConference")
            .Produces<ConferenceDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteConference)
            .WithName("DeleteConference")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapPost("/{id:guid}/follow", FollowConference)
            .WithName("FollowConference")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetConference(
        Guid id,
        IProfilesIntegrationService profilesIntegration,
        IQueryHandler<GetConferenceQuery, ConferenceDetailsDto?> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        Guid? currentProfileId = null;
        try
        {
            if (user.Identity?.IsAuthenticated == true)
            {
                var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
                if (Guid.TryParse(profile.ProfileId, out var profileId))
                {
                    currentProfileId = profileId;
                }
            }
        }
        catch (UnauthorizedException) { }
        catch (ProfileNotFoundException) { }

        var query = new GetConferenceQuery(id, currentProfileId);
        var result = await handler.Handle(query, cancellationToken);

        if (result == null)
        {
            return Results.NotFound(new { error = "Conference not found" });
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> ListConferences(
        IProfilesIntegrationService profilesIntegration,
        IQueryHandler<ListConferencesQuery, ListConferencesResult> handler,
        ClaimsPrincipal user,
        string? search = null,
        int? pageSize = null,
        int pageNumber = 1,
        bool showHidden = false,
        CancellationToken cancellationToken = default)
    {
        Guid? currentProfileId = null;
        try
        {
            if (user.Identity?.IsAuthenticated == true)
            {
                var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
                if (Guid.TryParse(profile.ProfileId, out var profileId))
                {
                    currentProfileId = profileId;
                }
            }
        }
        catch (UnauthorizedException) { }
        catch (ProfileNotFoundException) { }

        var query = new ListConferencesQuery(search, pageSize, pageNumber, showHidden, currentProfileId);
        var result = await handler.Handle(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateConference(
        CreateConferenceRequest request,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<CreateConferenceCommand, CreateConferenceResult> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Results.BadRequest(new { error = "Title is required" });
        }

        if (request.EndDate < request.StartDate)
        {
            return Results.BadRequest(new { error = "End date cannot be before start date" });
        }

        try
        {
            // Resolve the creating user's profile ID server-side
            Guid? createdByProfileId = null;
            try
            {
                var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
                if (Guid.TryParse(profile.ProfileId, out var profileId))
                {
                    createdByProfileId = profileId;
                }
            }
            catch (UnauthorizedException)
            {
                // Profile resolution failed; proceed without owner assignment
            }
            catch (ProfileNotFoundException)
            {
                // Profile not found; proceed without owner assignment
            }

            // Create command
            var command = new CreateConferenceCommand(
                request.Title.Trim(),
                request.City?.Trim() ?? "Unknown",
                request.Country?.Trim() ?? "Unknown",
                request.ImageUrl?.Trim(),
                request.StartDate,
                request.EndDate,
                request.SynchronizationSource,
                createdByProfileId);

            // Execute command
            var result = await handler.Handle(command, cancellationToken);

            return Results.Created($"/api/conferences/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> UpdateConference(
        Guid id,
        CreateConferenceRequest request,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<UpdateConferenceCommand, ConferenceDetailsDto> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Results.BadRequest(new { error = "Title is required" });
        }

        if (request.EndDate < request.StartDate)
        {
            return Results.BadRequest(new { error = "End date cannot be before start date" });
        }

        try
        {
            // Resolve profile ID and admin status server-side
            Guid? requestingProfileId = null;
            var isAdmin = IsAdminUser(user);
            try
            {
                var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
                if (Guid.TryParse(profile.ProfileId, out var profileId))
                {
                    requestingProfileId = profileId;
                }
            }
            catch (UnauthorizedException)
            {
                return Results.Unauthorized();
            }
            catch (ProfileNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }

            // Create command
            var command = new UpdateConferenceCommand(
                id,
                request.Title.Trim(),
                request.City?.Trim() ?? "Unknown",
                request.Country?.Trim() ?? "Unknown",
                request.ImageUrl?.Trim(),
                request.StartDate,
                request.EndDate,
                request.IsVisible,
                request.SynchronizationSource,
                requestingProfileId,
                isAdmin);

            // Execute command
            var result = await handler.Handle(command, cancellationToken);

            return Results.Ok(result);
        }
        catch (ForbiddenException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                title: "Visibility cannot be changed directly.",
                statusCode: StatusCodes.Status403Forbidden,
                type: ex.ErrorType);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { error = "Conference not found" });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> FollowConference(
        Guid id,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<FollowConferenceCommand> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            var command = new FollowConferenceCommand(
                id,
                Guid.Parse(profile.ProfileId));

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
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { error = "Conference not found" });
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> DeleteConference(
        Guid id,
        ICommandHandler<DeleteConferenceCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteConferenceCommand(id);
            var deleted = await handler.Handle(command, cancellationToken);

            if (!deleted)
            {
                return Results.NotFound(new { error = "Conference not found" });
            }

            return Results.NoContent();
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Checks whether the authenticated user has the admin permission.
    /// </summary>
    private static bool IsAdminUser(ClaimsPrincipal user)
    {
        var permissionsClaim = user.FindFirst("permissions");
        if (permissionsClaim == null)
        {
            return false;
        }

        var permissions = permissionsClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return permissions.Contains(Permissions.AdminAttendr);
    }
}
