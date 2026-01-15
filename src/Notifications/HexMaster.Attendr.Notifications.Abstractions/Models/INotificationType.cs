using HexMaster.Attendr.Notifications.Abstractions.Enums;

namespace HexMaster.Attendr.Notifications.Abstractions.Models;

/// <summary>
/// Abstraction for a notification type with its configuration and default channel settings.
/// </summary>
public interface INotificationType
{
    /// <summary>
    /// Unique identifier for the notification type.
    /// </summary>
    string TypeKey { get; }

    /// <summary>
    /// Display name for the notification type.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Description of when this notification is triggered.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The severity level of this notification type.
    /// </summary>
    NotificationSeverity Severity { get; }

    /// <summary>
    /// Whether this notification type allows stacking (combining multiple similar notifications).
    /// </summary>
    bool AllowsStacking { get; }

    /// <summary>
    /// Time window in seconds within which notifications can be stacked.
    /// Only applicable if AllowsStacking is true.
    /// </summary>
    int? StackWindowSeconds { get; }

    /// <summary>
    /// Default channel settings for this notification type.
    /// </summary>
    Dictionary<NotificationChannel, bool> DefaultChannelSettings { get; }

    /// <summary>
    /// Optional template for generating notification messages.
    /// </summary>
    string? MessageTemplate { get; }
}
