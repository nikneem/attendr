using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Repositories;

namespace HexMaster.Attendr.Profiles.Api.Endpoints;

/// <summary>
/// Integration endpoints for internal service-to-service communication.
/// These endpoints are anonymous and not exposed in OpenAPI documentation.
/// </summary>
public static class ProfilesIntegrationEndpoints
{
    /// <summary>
    /// Maps the profiles integration endpoints to the application.
    /// </summary>
    public static IEndpointRouteBuilder MapProfilesIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profiles-integration")
            .WithName("ProfilesIntegration")
            .ExcludeFromDescription();

        group.MapPost("/resolve", ResolveProfile)
            .WithName("ResolveProfile")
            .Produces<ResolveProfileResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapGet("/{profileId}", GetProfileDetails)
            .WithName("GetProfileDetailsIntegration")
            .Produces<ProfileDetailsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        return app;
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

    private static async Task<IResult> GetProfileDetails(
        string profileId,
        IProfileRepository repository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return Results.BadRequest(new { error = "ProfileId is required" });
        }

        try
        {
            var profile = await repository.GetByIdAsync(profileId, cancellationToken);
            if (profile is null)
            {
                return Results.NotFound();
            }

            // TODO: Add ProfilePictureUrl when available in the Profile domain model
            var result = new ProfileDetailsDto(
                profile.Id,
                profile.DisplayName,
                profile.FirstName,
                profile.LastName,
                profile.Email,
                null); // ProfilePictureUrl not yet implemented

            return Results.Ok(result);
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
