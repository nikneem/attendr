namespace HexMaster.Attendr.Groups.Data.Postgress;

/// <summary>
/// Configuration options for PostgreSQL connection.
/// </summary>
public sealed class PostgresOptions
{
    public const string SectionName = "Postgres";

    /// <summary>
    /// Gets or sets the schema name for the groups tables.
    /// </summary>
    public string SchemaName { get; set; } = "groups";

    /// <summary>
    /// Gets or sets the table name for storing groups.
    /// </summary>
    public string TableName { get; set; } = "groups";
}
