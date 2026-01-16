using HexMaster.Attendr.Notifications.Abstractions.Enums;

namespace HexMaster.Attendr.Notifications.Abstractions.DomainModels;

/// <summary>
/// Abstraction for a user's notification preferences.
/// </summary>
public interface INotificationPreferences
{
    /// <summary>
    /// The profile ID these preferences belong to.
    /// </summary>
    Guid ProfileId { get; }

    /// <summary>
    /// Per-type channel preferences. 
    /// Key is the notification type key, value is the channel settings.
    /// </summary>
    Dictionary<string, Dictionary<NotificationChannel, bool>> TypeChannelPreferences { get; }

    /// <summary>
    /// Global "do not disturb" mode. When set, temporarily disables all notifications.
    /// </summary>
    DateTime? DoNotDisturbUntil { get; set; }

    /// <summary>
    /// When these preferences were created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// When these preferences were last updated.
    /// </summary>
    DateTime? UpdatedAt { get; set; }
}
