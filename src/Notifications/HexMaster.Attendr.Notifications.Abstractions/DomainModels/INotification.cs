using HexMaster.Attendr.Notifications.Abstractions.Enums;

namespace HexMaster.Attendr.Notifications.Abstractions.DomainModels;

/// <summary>
/// Abstraction for a notification targeting a single profile.
/// </summary>
public interface INotification
{
    /// <summary>
    /// Unique identifier for the notification.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// The profile ID (user) this notification targets.
    /// </summary>
    Guid ProfileId { get; }

    /// <summary>
    /// The type key identifying the kind of notification.
    /// </summary>
    string TypeKey { get; }

    /// <summary>
    /// The severity level of the notification.
    /// </summary>
    NotificationSeverity Severity { get; }

    /// <summary>
    /// The notification title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// The notification message content.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Optional URL to link to related content.
    /// </summary>
    string? Url { get; }

    /// <summary>
    /// Optional actor (profile) who triggered this notification.
    /// </summary>
    Guid? ActorId { get; }

    /// <summary>
    /// Optional entity references for context.
    /// </summary>
    Dictionary<string, string>? EntityRefs { get; }

    /// <summary>
    /// Stack key for grouping similar notifications.
    /// </summary>
    string? StackKey { get; }

    /// <summary>
    /// Count of stacked notifications (1 for single, >1 for stacked).
    /// </summary>
    int Count { get; set; }

    /// <summary>
    /// When the notification was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// When the notification was last updated (for stacking).
    /// </summary>
    DateTime? LastOccurredAt { get; set; }

    /// <summary>
    /// When the notification was marked as read (null if unread).
    /// </summary>
    DateTime? ReadAt { get; set; }

    /// <summary>
    /// When the notification was marked as deleted (null if not deleted).
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// When the notification expires and should be cleaned up.
    /// </summary>
    DateTime? ExpiresAt { get; }

    /// <summary>
    /// Delivery status per channel.
    /// </summary>
    Dictionary<NotificationChannel, IChannelDeliveryInfo> ChannelDeliveries { get; }
}

/// <summary>
/// Abstraction for tracking delivery information for a specific channel.
/// </summary>
public interface IChannelDeliveryInfo
{
    /// <summary>
    /// Whether delivery is enabled for this channel.
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    /// Current delivery status.
    /// </summary>
    DeliveryStatus Status { get; set; }

    /// <summary>
    /// When delivery was attempted.
    /// </summary>
    DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Error message if delivery failed.
    /// </summary>
    string? ErrorMessage { get; set; }
}
