using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using HexMaster.Attendr.Notifications.Abstractions.Enums;

namespace HexMaster.Attendr.Notifications.DomainModels;

/// <summary>
/// Represents a user's notification preferences.
/// </summary>
public sealed class NotificationPreferences : INotificationPreferences
{
    /// <summary>
    /// The profile ID these preferences belong to.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// Per-type channel preferences. 
    /// Key is the notification type key, value is the channel settings.
    /// </summary>
    public required Dictionary<string, Dictionary<NotificationChannel, bool>> TypeChannelPreferences { get; init; }

    /// <summary>
    /// Global "do not disturb" mode. When set, temporarily disables all notifications.
    /// </summary>
    public DateTime? DoNotDisturbUntil { get; set; }

    /// <summary>
    /// When these preferences were created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When these preferences were last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
