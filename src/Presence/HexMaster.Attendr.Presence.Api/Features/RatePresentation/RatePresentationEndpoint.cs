using System.Security.Claims;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Integrations.Services;

namespace HexMaster.Attendr.Presence.Api.Features.RatePresentation;

public static class RatePresentationEndpoint
{
    public static IEndpointRouteBuilder MapRatePresentationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/presence/{conferenceId:guid}/rate/{presentationId:guid}", HandleAsync)
            .WithName("RatePresentation")
            .Accepts<RatePresentationDto>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid conferenceId,
        Guid presentationId,
        RatePresentationDto ratingDto,
        HttpContext context,
        RatePresentationService service,
        IProfilesIntegrationService profilesIntegration,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        var subjectId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? context.User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return Results.Unauthorized();
        }

        try
        {
            // Validate rating value
            if (ratingDto.Rating.HasValue && ratingDto.Rating.Value > 5)
            {
                return Results.BadRequest(new { error = "Rating must be between 0 and 5." });
            }

            // Resolve the Auth0 subject ID to a profile GUID
            var profile = await profilesIntegration.ResolveProfile(subjectId, cancellationToken);
            if (profile is null)
            {
                return Results.NotFound(new { error = "User profile not found. Please create a profile first." });
            }

            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            await service.ExecuteAsync(
                profileId,
                conferenceId,
                presentationId,
                ratingDto,
                cancellationToken);

            return Results.Ok(new { message = "Presentation rated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to rate presentation {PresentationId}", presentationId);
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rating presentation {PresentationId} for conference {ConferenceId}", presentationId, conferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
