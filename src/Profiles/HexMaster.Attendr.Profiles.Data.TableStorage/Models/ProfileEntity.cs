using Azure;
using Azure.Data.Tables;

namespace HexMaster.Attendr.Profiles.Data.TableStorage.Models;

/// <summary>
/// Azure Table Storage entity representation of a Profile aggregate.
/// Uses SubjectId as PartitionKey and Id as RowKey for efficient lookups.
/// </summary>
internal sealed class ProfileEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // SubjectId
    public string RowKey { get; set; } = string.Empty; // Id
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Id { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Employee { get; set; }
    public string? TagLine { get; set; }
    public bool IsSearchable { get; set; }
    public bool Enabled { get; set; }
}
