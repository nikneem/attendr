namespace HexMaster.Attendr.Notifications.Abstractions.DTOs;

/// <summary>
/// Detailed notification preferences combining user preferences with notification type configuration.
/// </summary>
public sealed class NotificationPreferencesDetailDto
{
    /// <summary>
    /// The profile ID these preferences belong to.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// When the user's preferences were last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// Do not disturb until this time (null if not active).
    /// </summary>
    public DateTimeOffset? DoNotDisturbUntil { get; init; }

    /// <summary>
    /// Detailed preferences for each notification type.
    /// </summary>
    public required List<NotificationTypePreferenceDto> NotificationTypes { get; init; }
}

/// <summary>
/// Preferences for a specific notification type with its configuration.
/// </summary>
public sealed class NotificationTypePreferenceDto
{
    /// <summary>
    /// The notification type key.
    /// </summary>
    public required string TypeKey { get; init; }

    /// <summary>
    /// The display name of the notification type.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Description of the notification type.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Channel preferences for this notification type.
    /// Only includes channels that are available for this type.
    /// </summary>
    public required Dictionary<string, ChannelPreferenceDto> ChannelPreferences { get; init; }
}

/// <summary>
/// Preference configuration for a specific channel.
/// </summary>
public sealed class ChannelPreferenceDto
{
    /// <summary>
    /// The channel name (InApp, Email, Push).
    /// </summary>
    public required string ChannelName { get; init; }

    /// <summary>
    /// Whether this channel is available for this notification type.
    /// </summary>
    public required bool IsAvailable { get; init; }

    /// <summary>
    /// The user's preference for this channel (enabled/disabled).
    /// Only meaningful if IsAvailable is true.
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// The default setting for this channel (from notification type configuration).
    /// </summary>
    public bool IsDefaultEnabled { get; init; }
}
