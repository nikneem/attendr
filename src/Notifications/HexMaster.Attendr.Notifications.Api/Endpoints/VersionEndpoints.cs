using System.Reflection;

namespace HexMaster.Attendr.Notifications.Api.Endpoints;

/// <summary>
/// Endpoints for version information.
/// </summary>
public static class VersionEndpoints
{
    public static IEndpointRouteBuilder MapVersionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/notifications/version", GetVersion)
            .WithName("GetNotificationsVersion")
            .AllowAnonymous()
            .Produces<VersionResponse>(StatusCodes.Status200OK);

        return app;
    }

    private static IResult GetVersion()
    {
        var version = typeof(Program).Assembly.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        return Results.Json(new VersionResponse { Version = version });
    }
}

/// <summary>
/// Version response DTO.
/// </summary>
public sealed class VersionResponse
{
    /// <summary>
    /// Gets the version of the service.
    /// </summary>
    public required string Version { get; init; }
}
