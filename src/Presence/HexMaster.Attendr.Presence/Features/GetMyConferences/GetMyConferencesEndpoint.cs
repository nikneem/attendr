using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Integrations.Services;

namespace HexMaster.Attendr.Presence.Features.GetMyConferences;

public static class GetMyConferencesEndpoint
{
    public static IEndpointRouteBuilder MapGetMyConferencesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/presence/my-conferences", HandleAsync)
            .WithName("GetMyConferences")
            .Produces<List<MyConferenceResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> HandleAsync(
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
}


