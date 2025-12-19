using Azure;
using Azure.Data.Tables;

namespace HexMaster.Attendr.Profiles.Data.TableStorage;

/// <summary>
/// Table entity representation of a profile.
/// </summary>
public sealed class ProfileTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string SubjectId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Employee { get; set; }
    public string? TagLine { get; set; }
    public bool Enabled { get; set; }
    public bool IsSearchable { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
}
