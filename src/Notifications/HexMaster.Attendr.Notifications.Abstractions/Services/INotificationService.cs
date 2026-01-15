using HexMaster.Attendr.Notifications.DomainModels;

namespace HexMaster.Attendr.Notifications.Abstractions.Services;

/// <summary>
/// Service for creating and managing notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates a new notification for a profile.
    /// Handles stacking logic if the notification type allows it.
    /// </summary>
    Task<Notification> CreateNotificationAsync(
        Guid profileId,
        string typeKey,
        string title,
        string message,
        string? url = null,
        Guid? actorId = null,
        Dictionary<string, string>? entityRefs = null,
        string? stackKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notifications for a profile with filtering options.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetNotificationsAsync(
        Guid profileId,
        bool includeRead = true,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single notification by ID.
    /// </summary>
    Task<Notification?> GetNotificationByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of unread notifications for a profile.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks multiple notifications as read.
    /// </summary>
    Task MarkMultipleAsReadAsync(IEnumerable<Guid> notificationIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all notifications for a profile as read.
    /// </summary>
    Task MarkAllAsReadAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as deleted (soft delete).
    /// </summary>
    Task MarkAsDeletedAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes expired notifications.
    /// </summary>
    Task DeleteExpiredNotificationsAsync(CancellationToken cancellationToken = default);
}
