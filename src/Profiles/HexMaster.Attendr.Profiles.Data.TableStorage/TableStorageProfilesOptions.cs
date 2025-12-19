namespace HexMaster.Attendr.Profiles.Data.TableStorage;

/// <summary>
/// Configuration options for the Profiles Azure Table Storage repository.
/// </summary>
public sealed class TableStorageProfilesOptions
{
    public const string SectionName = "Profiles:Storage";

    /// <summary>
    /// Azure Table Storage connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Table name to persist profiles. Defaults to "profiles".
    /// </summary>
    public string TableName { get; set; } = "profiles";
}
