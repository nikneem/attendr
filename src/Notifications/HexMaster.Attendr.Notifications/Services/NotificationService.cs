using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.DomainModels;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Notifications.Services;

/// <summary>
/// Implementation of INotificationService.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferencesRepository _preferencesRepository;
    private readonly INotificationTypeService _typeService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationPreferencesRepository preferencesRepository,
        INotificationTypeService typeService,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _preferencesRepository = preferencesRepository ?? throw new ArgumentNullException(nameof(preferencesRepository));
        _typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<INotification> CreateNotificationAsync(
        Guid profileId,
        string typeKey,
        string title,
        string message,
        string? url = null,
        Guid? actorId = null,
        Dictionary<string, string>? entityRefs = null,
        string? stackKey = null,
        CancellationToken cancellationToken = default)
    {
        var notificationType = _typeService.GetTypeByKey(typeKey);
        if (notificationType == null)
        {
            throw new InvalidOperationException($"Unknown notification type: {typeKey}");
        }

        // Get user preferences
        var preferences = await _preferencesRepository.GetByProfileIdAsync(profileId, cancellationToken);

        // Check if user has DND enabled
        if (preferences?.DoNotDisturbUntil.HasValue == true && preferences.DoNotDisturbUntil.Value > DateTime.UtcNow)
        {
            _logger.LogInformation(
                "Profile {ProfileId} has Do Not Disturb enabled until {Until}, notification will be created but marked as skipped",
                profileId, preferences.DoNotDisturbUntil.Value);
        }

        // Determine channel settings
        var channelSettings = GetChannelSettings(notificationType, preferences, typeKey);

        // Try to stack if allowed
        if (notificationType.AllowsStacking && !string.IsNullOrEmpty(stackKey))
        {
            var existingNotification = await _notificationRepository.FindStackableNotificationAsync(
                profileId, typeKey, stackKey, cancellationToken);

            if (existingNotification != null)
            {
                _logger.LogInformation(
                    "Stacking notification {TypeKey} for profile {ProfileId}, new count: {Count}",
                    typeKey, profileId, existingNotification.Count + 1);

                existingNotification.Count++;
                existingNotification.LastOccurredAt = DateTime.UtcNow;
                // Note: Message is immutable (init-only), keeping the original message from first occurrence

                await _notificationRepository.UpdateAsync(existingNotification, cancellationToken);
                return existingNotification;
            }
        }

        // Create new notification
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            TypeKey = typeKey,
            Severity = notificationType.Severity,
            Title = title,
            Message = message,
            Url = url,
            ActorId = actorId,
            EntityRefs = entityRefs,
            StackKey = stackKey,
            Count = 1,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // Default 30-day retention
            ChannelDeliveries = channelSettings
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);

        _logger.LogInformation(
            "Created notification {NotificationId} of type {TypeKey} for profile {ProfileId}",
            notification.Id, typeKey, profileId);

        return notification;
    }

    public async Task<IReadOnlyList<INotification>> GetNotificationsAsync(
        Guid profileId,
        bool includeRead = true,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetByProfileIdAsync(profileId, includeRead, includeDeleted, cancellationToken);
    }

    public async Task<INotification?> GetNotificationByIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetUnreadCountAsync(profileId, cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAsReadAsync(notificationId, cancellationToken);
        _logger.LogInformation("Marked notification {NotificationId} as read", notificationId);
    }

    public async Task MarkMultipleAsReadAsync(IEnumerable<Guid> notificationIds, CancellationToken cancellationToken = default)
    {
        foreach (var id in notificationIds)
        {
            await _notificationRepository.MarkAsReadAsync(id, cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetByProfileIdAsync(profileId, includeRead: false, cancellationToken: cancellationToken);
        foreach (var notification in notifications)
        {
            await _notificationRepository.MarkAsReadAsync(notification.Id, cancellationToken);
        }

        _logger.LogInformation("Marked all notifications as read for profile {ProfileId}", profileId);
    }

    public async Task MarkAsDeletedAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAsDeletedAsync(notificationId, cancellationToken);
        _logger.LogInformation("Marked notification {NotificationId} as deleted", notificationId);
    }

    public async Task DeleteExpiredNotificationsAsync(CancellationToken cancellationToken = default)
    {
        await _notificationRepository.DeleteExpiredAsync(cancellationToken);
        _logger.LogInformation("Deleted expired notifications");
    }

    private Dictionary<NotificationChannel, ChannelDeliveryInfo> GetChannelSettings(
        Abstractions.Models.INotificationType notificationType,
        INotificationPreferences? preferences,
        string typeKey)
    {
        var channelSettings = new Dictionary<NotificationChannel, ChannelDeliveryInfo>();

        foreach (var channel in Enum.GetValues<NotificationChannel>())
        {
            // Check if user has custom preference for this type/channel
            bool enabled = notificationType.DefaultChannelSettings.TryGetValue(channel, out var defaultEnabled) && defaultEnabled;

            if (preferences?.TypeChannelPreferences.TryGetValue(typeKey, out var userChannelPrefs) == true)
            {
                if (userChannelPrefs.TryGetValue(channel, out var userEnabled))
                {
                    enabled = userEnabled;
                }
            }

            // If DND is active, mark as skipped
            var status = DeliveryStatus.Pending;
            if (preferences?.DoNotDisturbUntil.HasValue == true && preferences.DoNotDisturbUntil.Value > DateTime.UtcNow)
            {
                status = DeliveryStatus.Skipped;
            }
            else if (!enabled)
            {
                status = DeliveryStatus.Skipped;
            }

            channelSettings[channel] = new ChannelDeliveryInfo
            {
                Enabled = enabled,
                Status = status
            };
        }

        return channelSettings;
    }
}
