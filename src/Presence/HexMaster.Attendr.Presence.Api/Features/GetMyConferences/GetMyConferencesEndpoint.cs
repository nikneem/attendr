using System.Security.Claims;
using HexMaster.Attendr.Presence.Services;
using HexMaster.Attendr.Profiles.Integrations.Services;

namespace HexMaster.Attendr.Presence.Api.Features.GetMyConferences;

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
}
