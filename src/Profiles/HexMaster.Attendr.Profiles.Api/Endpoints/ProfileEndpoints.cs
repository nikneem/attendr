using System.Security.Claims;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.CreateProfile;
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

        group.MapPost("/", CreateProfile)
            .WithName("CreateProfile")
            .Produces<CreateProfileResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        var integrationGroup = app.MapGroup("/api/profiles-integration")
            .WithName("ProfilesIntegration");

        integrationGroup.MapPost("/resolve", ResolveProfile)
            .WithName("ResolveProfile")
            .Produces<ResolveProfileResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
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

        // Extract SubjectId from JWT token
        var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subjectId))
            return Results.Unauthorized();

        // Create display name from first and last name
        var displayName = $"{request.FirstName.Trim()} {request.LastName.Trim()}";

        try
        {
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
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> ResolveProfile(
        ResolveProfileRequest request,
        IProfileRepository repository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SubjectId))
        {
            return Results.BadRequest(new { error = "SubjectId is required" });
        }

        try
        {
            var profile = await repository.GetBySubjectIdAsync(request.SubjectId, cancellationToken);
            if (profile is null)
            {
                return Results.NotFound();
            }

            var result = new ResolveProfileResult(profile.Id, profile.DisplayName);
            return Results.Ok(result);
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
