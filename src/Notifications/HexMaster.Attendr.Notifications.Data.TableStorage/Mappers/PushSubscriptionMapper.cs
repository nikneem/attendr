using HexMaster.Attendr.Notifications.Data.TableStorage.Entities;
using HexMaster.Attendr.Notifications.DomainModels;

namespace HexMaster.Attendr.Notifications.Data.TableStorage.Mappers;

/// <summary>
/// Maps push subscriptions between domain and table entities.
/// </summary>
internal static class PushSubscriptionMapper
{
    public static PushSubscriptionEntity ToEntity(PushSubscription subscription)
    {
        return new PushSubscriptionEntity
        {
            PartitionKey = subscription.ProfileId.ToString(),
            RowKey = PushSubscriptionEntity.CreateRowKey(subscription.Endpoint),
            ProfileId = subscription.ProfileId.ToString(),
            Endpoint = subscription.Endpoint,
            P256dh = subscription.P256dh,
            Auth = subscription.Auth,
            UserAgent = subscription.UserAgent,
            ExpirationTime = subscription.ExpirationTime,
            CreatedAt = subscription.CreatedAt,
            UpdatedAt = subscription.UpdatedAt
        };
    }

    public static PushSubscription ToDomain(PushSubscriptionEntity entity)
    {
        return new PushSubscription
        {
            ProfileId = Guid.Parse(entity.ProfileId),
            Endpoint = entity.Endpoint,
            P256dh = entity.P256dh,
            Auth = entity.Auth,
            UserAgent = entity.UserAgent,
            ExpirationTime = entity.ExpirationTime,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
