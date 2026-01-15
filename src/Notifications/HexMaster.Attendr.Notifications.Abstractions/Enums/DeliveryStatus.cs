namespace HexMaster.Attendr.Notifications.Abstractions.Enums;

/// <summary>
/// Represents the delivery status of a notification for a specific channel.
/// </summary>
public enum DeliveryStatus
{
    /// <summary>
    /// Notification is pending delivery.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Notification has been successfully delivered.
    /// </summary>
    Delivered = 1,

    /// <summary>
    /// Notification delivery failed.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Notification was skipped based on user preferences.
    /// </summary>
    Skipped = 3
}
