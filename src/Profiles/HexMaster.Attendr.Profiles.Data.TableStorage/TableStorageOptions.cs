namespace HexMaster.Attendr.Profiles.Data.TableStorage;

/// <summary>
/// Configuration options for Azure Table Storage connection.
/// </summary>
public sealed class TableStorageOptions
{
    public const string SectionName = "TableStorage";

    /// <summary>
    /// Gets or sets the Azure Table Storage connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the table name for storing profiles.
    /// </summary>
    public string TableName { get; set; } = "profiles";
}
