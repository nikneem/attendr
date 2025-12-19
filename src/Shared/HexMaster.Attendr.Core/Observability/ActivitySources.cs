using System.Diagnostics;

namespace HexMaster.Attendr.Core.Observability;

/// <summary>
/// Central registry for ActivitySources used across the Attendr application.
/// ActivitySources are used to create Activities (spans) for distributed tracing.
/// </summary>
public static class ActivitySources
{
    private const string Namespace = "HexMaster.Attendr";
    private const string Version = "1.0.0";

    /// <summary>
    /// ActivitySource for Profiles module operations.
    /// Used for tracing profile creation, retrieval, and management operations.
    /// </summary>
    public static readonly ActivitySource Profiles = new($"{Namespace}.Profiles", Version);

    /// <summary>
    /// ActivitySource for Groups module operations.
    /// Used for tracing group creation, membership management, and related operations.
    /// </summary>
    public static readonly ActivitySource Groups = new($"{Namespace}.Groups", Version);

    /// <summary>
    /// ActivitySource for Conferences module operations.
    /// Used for tracing conference management and related operations.
    /// </summary>
    public static readonly ActivitySource Conferences = new($"{Namespace}.Conferences", Version);

    /// <summary>
    /// ActivitySource for proxy/gateway operations.
    /// Used for tracing reverse proxy request flows.
    /// </summary>
    public static readonly ActivitySource Proxy = new($"{Namespace}.Proxy", Version);
}
