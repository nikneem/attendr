using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using HexMaster.Attendr.Notifications.Abstractions.Enums;

namespace HexMaster.Attendr.Notifications.Abstractions.Repositories;

/// <summary>
/// Repository for managing user notification preferences.
/// </summary>
public interface INotificationPreferencesRepository
{
    /// <summary>
    /// Gets notification preferences for a profile.
    /// Returns null if no preferences exist yet (should use defaults).
    /// </summary>
    Task<INotificationPreferences?> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates notification preferences for a profile.
    /// </summary>
    Task UpsertAsync(INotificationPreferences preferences, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates channel preferences for a specific notification type.
    /// </summary>
    Task UpdateTypeChannelPreferencesAsync(
        Guid profileId,
        string typeKey,
        Dictionary<NotificationChannel, bool> channelSettings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets or clears the "do not disturb" mode for a profile.
    /// </summary>
    Task UpdateDoNotDisturbAsync(
        Guid profileId,
        DateTime? doNotDisturbUntil,
        CancellationToken cancellationToken = default);
}
