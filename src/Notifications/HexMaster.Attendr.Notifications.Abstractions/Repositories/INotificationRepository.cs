using HexMaster.Attendr.Notifications.Abstractions.DomainModels;

namespace HexMaster.Attendr.Notifications.Abstractions.Repositories;

/// <summary>
/// Repository for managing notification data.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Gets a notification by its ID.
    /// </summary>
    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all notifications for a profile with optional filtering.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetByProfileIdAsync(
        Guid profileId,
        bool includeRead = true,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to find an existing unread notification that can be stacked with the given criteria.
    /// </summary>
    Task<Notification?> FindStackableNotificationAsync(
        Guid profileId,
        string typeKey,
        string stackKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new notification.
    /// </summary>
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing notification (used for stacking, marking as read/deleted, etc.).
    /// </summary>
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets count of unread notifications for a profile.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as deleted.
    /// </summary>
    Task MarkAsDeletedAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes expired notifications.
    /// </summary>
    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
