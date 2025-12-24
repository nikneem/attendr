namespace HexMaster.Attendr.Profiles.Integrations;

public sealed class ProfilesIntegrationOptions
{
    public const string SectionName = "Profiles:Integration";

    /// <summary>
    /// Default TTL for resolved profiles, in seconds.
    /// </summary>
    public int CacheTtlSeconds { get; set; } = 300;
}
