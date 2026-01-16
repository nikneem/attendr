using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using HexMaster.Attendr.Notifications.Abstractions.DTOs;
using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.DomainModels;
using HexMaster.Attendr.Notifications.Models;

namespace HexMaster.Attendr.Notifications.Mappers;

public static class NotificationDtoMapper
{
    public static NotificationDto ToDto(INotification notification)
    {
        // Cast to concrete type
        var concreteNotification = notification as Notification
            ?? throw new InvalidOperationException($"Expected {nameof(Notification)} but got {notification.GetType().Name}");
        return ToDto(concreteNotification);
    }

    public static NotificationDto ToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            ProfileId = notification.ProfileId,
            TypeKey = notification.TypeKey,
            Severity = notification.Severity.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            Url = notification.Url,
            ActorId = notification.ActorId,
            EntityRefs = notification.EntityRefs,
            Count = notification.Count,
            CreatedAt = notification.CreatedAt,
            LastOccurredAt = notification.LastOccurredAt,
            ReadAt = notification.ReadAt,
            ChannelDeliveries = notification.ChannelDeliveries.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => new ChannelDeliveryDto
                {
                    Enabled = kvp.Value.Enabled,
                    Status = kvp.Value.Status.ToString(),
                    DeliveredAt = kvp.Value.DeliveredAt,
                    ErrorMessage = kvp.Value.ErrorMessage
                })
        };
    }

    public static NotificationTypeDto ToDto(NotificationType type)
    {
        return new NotificationTypeDto
        {
            TypeKey = type.TypeKey,
            DisplayName = type.DisplayName,
            Description = type.Description,
            Severity = type.Severity.ToString(),
            AllowsStacking = type.AllowsStacking,
            DefaultChannelSettings = type.DefaultChannelSettings.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value)
        };
    }

    public static NotificationPreferencesDto ToDto(INotificationPreferences preferences)
    {
        // Cast to concrete type
        var concretePreferences = preferences as NotificationPreferences
            ?? throw new InvalidOperationException($"Expected {nameof(NotificationPreferences)} but got {preferences.GetType().Name}");
        return ToDto(concretePreferences);
    }

    public static NotificationPreferencesDto ToDto(NotificationPreferences preferences)
    {
        return new NotificationPreferencesDto
        {
            ProfileId = preferences.ProfileId,
            TypeChannelPreferences = preferences.TypeChannelPreferences.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToDictionary(
                    c => c.Key.ToString(),
                    c => c.Value)),
            DoNotDisturbUntil = preferences.DoNotDisturbUntil
        };
    }

    public static NotificationPreferences ToDomain(Guid profileId, Dictionary<string, Dictionary<string, bool>> typeChannelPreferences)
    {
        return new NotificationPreferences
        {
            ProfileId = profileId,
            TypeChannelPreferences = typeChannelPreferences.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToDictionary(
                    c => Enum.Parse<NotificationChannel>(c.Key),
                    c => c.Value)),
            CreatedAt = DateTime.UtcNow
        };
    }
}
