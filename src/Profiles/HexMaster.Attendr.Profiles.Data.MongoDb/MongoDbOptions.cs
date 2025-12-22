namespace HexMaster.Attendr.Profiles.Data.MongoDb;

/// <summary>
/// Configuration options for MongoDB connection.
/// </summary>
public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    /// <summary>
    /// Gets or sets the MongoDB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string DatabaseName { get; set; } = "attendr_profiles";
}
