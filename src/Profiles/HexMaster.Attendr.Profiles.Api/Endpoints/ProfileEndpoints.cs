using System.Security.Claims;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Features.CreateProfile;
using HexMaster.Attendr.Profiles.Features.UpdateProfile;
using HexMaster.Attendr.Profiles.Repositories;

namespace HexMaster.Attendr.Profiles.Api.Endpoints;

/// <summary>
/// Extension methods to map profile endpoints.
/// </summary>
public static class ProfileEndpoints
{
    /// <summary>
    /// Maps the profile endpoints to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profiles")
            .WithName("Profiles");

        group.MapGet("/", GetProfile)
            .WithName("GetProfile")
            .Produces<ProfileDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapPost("/", CreateProfile)
            .WithName("CreateProfile")
            .Produces<CreateProfileResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPut("/", UpdateProfile)
            .WithName("UpdateProfile")
            .Produces<UpdateProfileResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetProfile(
        ClaimsPrincipal user,
        IProfileRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(subjectId))
            {
                return Results.Unauthorized();
            }

            var profile = await repository.GetBySubjectIdAsync(subjectId, cancellationToken);
            if (profile is null)
            {
                return Results.NotFound();
            }

            var result = new ProfileDetailsDto(
                profile.Id,
                profile.DisplayName,
                profile.FirstName,
                profile.LastName,
                profile.Email,
                null,
                profile.TagLine,
                profile.IsSearchable);

            return Results.Ok(result);
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> CreateProfile(
        CreateProfileRequest request,
        ICommandHandler<CreateProfileCommand, CreateProfileResult> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return Results.BadRequest(new { error = "FirstName is required" });

        if (string.IsNullOrWhiteSpace(request.LastName))
            return Results.BadRequest(new { error = "LastName is required" });

        if (string.IsNullOrWhiteSpace(request.Email))
            return Results.BadRequest(new { error = "Email is required" });

        try
        {
            // Extract SubjectId from JWT token - throws UnauthorizedException if not found
            var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(subjectId))
                throw new UnauthorizedException();

            // Create display name from first and last name
            var displayName = $"{request.FirstName.Trim()} {request.LastName.Trim()}";

            // Create and handle the command
            var command = new CreateProfileCommand(
                subjectId,
                displayName,
                request.FirstName,
                request.LastName,
                request.Email
            );

            var result = await handler.Handle(command, cancellationToken);

            return Results.Created($"/api/profiles/{result.ProfileId}", result);
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
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

    private static async Task<IResult> UpdateProfile(
        UpdateProfileRequest request,
        ICommandHandler<UpdateProfileCommand, UpdateProfileResult> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(subjectId))
            {
                return Results.Unauthorized();
            }

            var command = new UpdateProfileCommand(
                subjectId,
                request.DisplayName,
                request.FirstName,
                request.LastName,
                request.TagLine,
                request.IsSearchable);

            var result = await handler.Handle(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
