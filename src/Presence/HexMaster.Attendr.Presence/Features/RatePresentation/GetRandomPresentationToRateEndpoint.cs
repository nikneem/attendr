using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;using Microsoft.AspNetCore.Mvc;using Microsoft.Extensions.Logging;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Integrations.Services;

namespace HexMaster.Attendr.Presence.Features.RatePresentation;

public static class GetRandomPresentationToRateEndpoint
{
    public static IEndpointRouteBuilder MapGetRandomPresentationToRateEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/presence/{conferenceId:guid}/rate", HandleAsync)
            .WithName("GetRandomPresentationToRate")
            .Produces<PresentationToRateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid conferenceId,
        HttpContext context,
        IQueryHandler<GetRandomPresentationToRateQuery, PresentationToRateDto?> handler,
        IProfilesIntegrationService profilesIntegration,
        [FromServices] ILogger logger,
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
}


