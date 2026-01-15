using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.Models;

namespace HexMaster.Attendr.Notifications.Models;

/// <summary>
/// Defines a type of notification with its configuration and default channel settings.
/// </summary>
public sealed class NotificationType : INotificationType
{
    /// <summary>
    /// Unique identifier for the notification type.
    /// </summary>
    public required string TypeKey { get; init; }

    /// <summary>
    /// Display name for the notification type.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Description of when this notification is triggered.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The severity level of this notification type.
    /// </summary>
    public required NotificationSeverity Severity { get; init; }

    /// <summary>
    /// Whether this notification type allows stacking (combining multiple similar notifications).
    /// </summary>
    public required bool AllowsStacking { get; init; }

    /// <summary>
    /// Time window in seconds within which notifications can be stacked.
    /// Only applicable if AllowsStacking is true.
    /// </summary>
    public int? StackWindowSeconds { get; init; }

    /// <summary>
    /// Default channel settings for this notification type.
    /// </summary>
    public required Dictionary<NotificationChannel, bool> DefaultChannelSettings { get; init; }

    /// <summary>
    /// Optional template for generating notification messages.
    /// </summary>
    public string? MessageTemplate { get; init; }
}
