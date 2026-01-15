using HexMaster.Attendr.Notifications.Abstractions.Enums;

namespace HexMaster.Attendr.Notifications.Abstractions.DomainModels;

/// <summary>
/// Represents a notification targeting a single profile.
/// </summary>
public sealed class Notification
{
    /// <summary>
    /// Unique identifier for the notification.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The profile ID (user) this notification targets.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// The type key identifying the kind of notification.
    /// </summary>
    public required string TypeKey { get; init; }

    /// <summary>
    /// The severity level of the notification.
    /// </summary>
    public required NotificationSeverity Severity { get; init; }

    /// <summary>
    /// The notification title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The notification message content.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional URL to link to related content.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Optional actor (profile) who triggered this notification.
    /// </summary>
    public Guid? ActorId { get; init; }

    /// <summary>
    /// Optional entity references for context.
    /// </summary>
    public Dictionary<string, string>? EntityRefs { get; init; }

    /// <summary>
    /// Stack key for grouping similar notifications.
    /// </summary>
    public string? StackKey { get; init; }

    /// <summary>
    /// Count of stacked notifications (1 for single, >1 for stacked).
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// When the notification was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the notification was last updated (for stacking).
    /// </summary>
    public DateTime? LastOccurredAt { get; set; }

    /// <summary>
    /// When the notification was marked as read (null if unread).
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// When the notification was marked as deleted (null if not deleted).
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// When the notification expires and should be cleaned up.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// Delivery status per channel.
    /// </summary>
    public required Dictionary<NotificationChannel, ChannelDeliveryInfo> ChannelDeliveries { get; init; }
}

/// <summary>
/// Tracks delivery information for a specific channel.
/// </summary>
public sealed class ChannelDeliveryInfo
{
    /// <summary>
    /// Whether delivery is enabled for this channel.
    /// </summary>
    public required bool Enabled { get; init; }

    /// <summary>
    /// Current delivery status.
    /// </summary>
    public required DeliveryStatus Status { get; set; }

    /// <summary>
    /// When delivery was attempted.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Error message if delivery failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
