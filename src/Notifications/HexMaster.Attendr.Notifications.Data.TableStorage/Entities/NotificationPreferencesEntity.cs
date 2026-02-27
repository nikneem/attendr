using Azure;
using Azure.Data.Tables;

namespace HexMaster.Attendr.Notifications.Data.TableStorage.Entities;

/// <summary>
/// Azure Table Storage entity for notification preferences.
/// PartitionKey: ProfileId
/// RowKey: "preferences" (constant)
/// </summary>
internal sealed class NotificationPreferencesEntity : ITableEntity
{
    public const string PreferencesRowKey = "preferences";

    public string PartitionKey { get; set; } = string.Empty; // ProfileId
    public string RowKey { get; set; } = PreferencesRowKey;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string ProfileId { get; set; } = string.Empty;

    // Stored as JSON: Dictionary<string, Dictionary<NotificationChannel, bool>>
    public string TypeChannelPreferencesJson { get; set; } = string.Empty;

    public DateTimeOffset? DoNotDisturbUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
