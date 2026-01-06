using System.Security.Claims;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Features.GetMyConferences;
using HexMaster.Attendr.Presence.Features.RatePresentation;
using HexMaster.Attendr.Profiles.Integrations.Services;

namespace HexMaster.Attendr.Presence.Api.Endpoints;

/// <summary>
/// Extension methods to map presence endpoints.
/// </summary>
public static class PresenceEndpoints
{
    /// <summary>
    /// Maps the presence endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static IEndpointRouteBuilder MapPresenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/presence")
            .WithName("Presence")
            .RequireAuthorization();

        group.MapGet("/my-conferences", GetMyConferences)
            .WithName("GetMyConferences")
            .Produces<List<MyConferenceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{conferenceId:guid}/rate", GetRandomPresentationToRate)
            .WithName("GetRandomPresentationToRate")
            .Produces<PresentationToRateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{conferenceId:guid}/rate/{presentationId:guid}", RatePresentation)
            .WithName("RatePresentation")
            .Accepts<RatePresentationDto>("application/json")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetMyConferences(
        HttpContext context,
        IQueryHandler<GetMyConferencesQuery, List<MyConferenceResponse>> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("GetMyConferencesEndpoint");
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

            var result = await handler.Handle(new GetMyConferencesQuery(profileId), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving conferences for user {SubjectId}", subjectId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetRandomPresentationToRate(
        Guid conferenceId,
        HttpContext context,
        IQueryHandler<GetRandomPresentationToRateQuery, PresentationToRateDto?> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("GetRandomPresentationToRateEndpoint");
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

            var result = await handler.Handle(
                new GetRandomPresentationToRateQuery(profileId, conferenceId),
                cancellationToken);

            if (result == null)
            {
                return Results.NoContent();
            }

            return Results.Ok(result);
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
        ICommandHandler<RatePresentationCommand> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("RatePresentationEndpoint");
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

            await handler.Handle(
                new RatePresentationCommand(profileId, conferenceId, presentationId, ratingDto),
                cancellationToken);

            return Results.Accepted();
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
