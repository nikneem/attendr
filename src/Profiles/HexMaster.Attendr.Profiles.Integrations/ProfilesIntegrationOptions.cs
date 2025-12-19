namespace HexMaster.Attendr.Profiles.Integrations;

public sealed class ProfilesIntegrationOptions
{
    public const string SectionName = "Profiles:Integration";

    /// <summary>
    /// Base URL of the Profiles API (e.g., https://localhost:5001).
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Default TTL for resolved profiles, in seconds.
    /// </summary>
    public int CacheTtlSeconds { get; set; } = 300;
}
