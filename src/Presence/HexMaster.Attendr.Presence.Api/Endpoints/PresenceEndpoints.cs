using System.Security.Claims;
using HexMaster.Attendr.Presence.Services;

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

        return app;
    }

    private static async Task<IResult> GetMyConferences(
        HttpContext context,
        IConferencePresenceRepository repository,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        var profileIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (profileIdClaim is null || !Guid.TryParse(profileIdClaim.Value, out var profileId))
        {
            return Results.Unauthorized();
        }

        try
        {
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
            logger.LogError(ex, "Error retrieving conferences for profile {ProfileId}", profileId);
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
