using HexMaster.Attendr.Notifications.Abstractions.DomainModels;

namespace HexMaster.Attendr.Notifications.Abstractions.Repositories;

/// <summary>
/// Repository for managing push notification subscriptions.
/// </summary>
public interface IPushSubscriptionRepository
{
    /// <summary>
    /// Creates or updates a push subscription for a profile.
    /// </summary>
    Task UpsertAsync(IPushSubscription subscription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all push subscriptions for the specified profile.
    /// </summary>
    Task<IReadOnlyList<IPushSubscription>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a push subscription for the specified profile and endpoint.
    /// </summary>
    Task DeleteAsync(Guid profileId, string endpoint, CancellationToken cancellationToken = default);
}
