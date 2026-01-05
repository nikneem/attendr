using System.Security.Claims;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Api.Services;
using HexMaster.Attendr.Presence.Services;
using HexMaster.Attendr.Profiles.Integrations.Services;

namespace HexMaster.Attendr.Presence.Api.Endpoints;

/// <summary>
/// Endpoints for presence-related operations.
/// </summary>
public static class PresenceEndpoints
{
    /// <summary>
    /// Maps presence endpoints to the application.
    /// These endpoints require authentication.
    /// </summary>
    public static IEndpointRouteBuilder MapPresenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/presence")
            .WithName("Presence")
            .RequireAuthorization();

        group.MapGet("/my-conferences", GetMyConferences)
            .WithName("GetMyConferences")
            .Produces<List<MyConferenceResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{conferenceId:guid}/rate", GetRandomPresentationToRate)
            .WithName("GetRandomPresentationToRate")
            .Produces<PresentationToRateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{conferenceId:guid}/rate/{presentationId:guid}", RatePresentation)
            .WithName("RatePresentation")
            .Accepts<RatePresentationDto>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetMyConferences(
        HttpContext context,
        IConferencePresenceRepository repository,
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

            var allPresences = await repository.GetByProfileIdAsync(profileId, cancellationToken);
            var now = DateTime.UtcNow;

            var currentAndFuture = allPresences
                .Where(p => p.EndDate >= DateOnly.FromDateTime(now))
                .OrderBy(p => p.StartDate)
                .Select(p => new MyConferenceResponse(
                    p.ConferenceId,
                    p.ConferenceName,
                    p.Location,
                    p.StartDate.ToDateTime(TimeOnly.MinValue),
                    p.EndDate.ToDateTime(TimeOnly.MaxValue),
                    p.IsAttending))
                .ToList();

            return Results.Ok(currentAndFuture);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving conferences for subject {SubjectId}", subjectId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetRandomPresentationToRate(
        Guid conferenceId,
        HttpContext context,
        IPresentationRatingService ratingService,
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

            var presentation = await ratingService.GetRandomUnratedPresentationAsync(
                profileId,
                conferenceId,
                cancellationToken);

            if (presentation == null)
            {
                return Results.NoContent();
            }

            return Results.Ok(presentation);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting random presentation to rate for conference {ConferenceId}", conferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> RatePresentation(
        Guid conferenceId,
        Guid presentationId,
        RatePresentationDto ratingDto,
        HttpContext context,
        IPresentationRatingService ratingService,
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

            await ratingService.RatePresentationAsync(
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

/// <summary>
/// Response model for user's conferences.
/// </summary>
public sealed record MyConferenceResponse(
    Guid ConferenceId,
    string ConferenceName,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAttending);
