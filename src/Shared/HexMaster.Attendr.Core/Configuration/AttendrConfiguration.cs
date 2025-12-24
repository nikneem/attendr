namespace HexMaster.Attendr.Core.Configuration;

/// <summary>
/// Configuration for Attendr service integrations.
/// </summary>
public sealed class AttendrConfiguration
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Attendr";

    /// <summary>
    /// Integration endpoints configuration.
    /// </summary>
    public IntegrationEndpoints Integration { get; set; } = new();
}

/// <summary>
/// Integration service endpoints.
/// </summary>
public sealed class IntegrationEndpoints
{
    /// <summary>
    /// Profiles service base URL.
    /// </summary>
    public string Profiles { get; set; } = string.Empty;

    /// <summary>
    /// Groups service base URL.
    /// </summary>
    public string Groups { get; set; } = string.Empty;

    /// <summary>
    /// Conferences service base URL.
    /// </summary>
    public string Conferences { get; set; } = string.Empty;
}
