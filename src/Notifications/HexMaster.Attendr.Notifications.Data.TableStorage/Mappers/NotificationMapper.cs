using System.Text.Json;
using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Data.TableStorage.Entities;

namespace HexMaster.Attendr.Notifications.Data.TableStorage.Mappers;

/// <summary>
/// Maps between Notification domain model and NotificationEntity.
/// </summary>
internal static class NotificationMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static NotificationEntity ToEntity(Notification notification)
    {
        return new NotificationEntity
        {
            PartitionKey = notification.ProfileId.ToString(),
            RowKey = notification.Id.ToString(),
            ProfileId = notification.ProfileId.ToString(),
            NotificationId = notification.Id.ToString(),
            TypeKey = notification.TypeKey,
            Severity = (int)notification.Severity,
            Title = notification.Title,
            Message = notification.Message,
            Url = notification.Url,
            ActorId = notification.ActorId?.ToString(),
            EntityRefsJson = notification.EntityRefs != null
                ? JsonSerializer.Serialize(notification.EntityRefs, JsonOptions)
                : null,
            StackKey = notification.StackKey,
            Count = notification.Count,
            CreatedAt = notification.CreatedAt,
            LastOccurredAt = notification.LastOccurredAt,
            ReadAt = notification.ReadAt,
            DeletedAt = notification.DeletedAt,
            ExpiresAt = notification.ExpiresAt,
            ChannelDeliveriesJson = JsonSerializer.Serialize(notification.ChannelDeliveries, JsonOptions)
        };
    }

    public static Notification ToDomain(NotificationEntity entity)
    {
        return new Notification
        {
            Id = Guid.Parse(entity.NotificationId),
            ProfileId = Guid.Parse(entity.ProfileId),
            TypeKey = entity.TypeKey,
            Severity = (NotificationSeverity)entity.Severity,
            Title = entity.Title,
            Message = entity.Message,
            Url = entity.Url,
            ActorId = !string.IsNullOrEmpty(entity.ActorId) ? Guid.Parse(entity.ActorId) : null,
            EntityRefs = !string.IsNullOrEmpty(entity.EntityRefsJson)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(entity.EntityRefsJson, JsonOptions)
                : null,
            StackKey = entity.StackKey,
            Count = entity.Count,
            CreatedAt = entity.CreatedAt,
            LastOccurredAt = entity.LastOccurredAt,
            ReadAt = entity.ReadAt,
            DeletedAt = entity.DeletedAt,
            ExpiresAt = entity.ExpiresAt,
            ChannelDeliveries = JsonSerializer.Deserialize<Dictionary<NotificationChannel, ChannelDeliveryInfo>>(
                entity.ChannelDeliveriesJson, JsonOptions) ?? new Dictionary<NotificationChannel, ChannelDeliveryInfo>()
        };
    }
}
