using Azure;
using Azure.Data.Tables;
using HexMaster.Attendr.Notifications.Abstractions.Enums;

namespace HexMaster.Attendr.Notifications.Data.TableStorage.Entities;

/// <summary>
/// Azure Table Storage entity for notifications.
/// PartitionKey: ProfileId
/// RowKey: NotificationId
/// </summary>
internal sealed class NotificationEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // ProfileId
    public string RowKey { get; set; } = string.Empty; // NotificationId
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // Core properties
    public string ProfileId { get; set; } = string.Empty;
    public string NotificationId { get; set; } = string.Empty;
    public string TypeKey { get; set; } = string.Empty;
    public int Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? ActorId { get; set; }

    // Entity refs stored as JSON string
    public string? EntityRefsJson { get; set; }

    // Stacking
    public string? StackKey { get; set; }
    public int Count { get; set; } = 1;

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? LastOccurredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Channel delivery info stored as JSON strings
    public string ChannelDeliveriesJson { get; set; } = string.Empty;
}
