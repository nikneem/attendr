using Azure;
using Azure.Data.Tables;
using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.Data.TableStorage.Entities;
using HexMaster.Attendr.Notifications.Data.TableStorage.Mappers;
using HexMaster.Attendr.Notifications.DomainModels;

namespace HexMaster.Attendr.Notifications.Data.TableStorage.Repositories;

/// <summary>
/// Azure Table Storage implementation of INotificationRepository.
/// </summary>
public sealed class TableStorageNotificationRepository : INotificationRepository
{
    private const string TableName = AspireConstants.TableStorage.Notifications;
    private readonly TableServiceClient _tableServiceClient;

    public TableStorageNotificationRepository(TableServiceClient tableServiceClient)
    {
        _tableServiceClient = tableServiceClient ?? throw new ArgumentNullException(nameof(tableServiceClient));
    }

    public async Task<INotification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var tableClient = await GetTableClientAsync(cancellationToken);

        // We need to query across partitions since we only have the notification ID
        var filter = $"RowKey eq '{notificationId}'";

        await foreach (var entity in tableClient.QueryAsync<NotificationEntity>(filter, cancellationToken: cancellationToken))
        {
            return NotificationMapper.ToDomain(entity);
        }

        return null;
    }

    public async Task<IReadOnlyList<INotification>> GetByProfileIdAsync(
        Guid profileId,
        bool includeRead = true,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var tableClient = await GetTableClientAsync(cancellationToken);
        var filter = $"PartitionKey eq '{profileId}'";

        if (!includeRead)
        {
            filter += " and ReadAt eq null";
        }

        if (!includeDeleted)
        {
            filter += " and DeletedAt eq null";
        }

        var notifications = new List<Notification>();
        await foreach (var entity in tableClient.QueryAsync<NotificationEntity>(filter, cancellationToken: cancellationToken))
        {
            notifications.Add(NotificationMapper.ToDomain(entity));
        }

        return notifications.OrderByDescending(n => n.CreatedAt).ToList();
    }

    public async Task<INotification?> FindStackableNotificationAsync(
        Guid profileId,
        string typeKey,
        string stackKey,
        CancellationToken cancellationToken = default)
    {
        var tableClient = await GetTableClientAsync(cancellationToken);

        // Find unread notification with matching profile, type, and stack key
        var filter = $"PartitionKey eq '{profileId}' and TypeKey eq '{typeKey}' and StackKey eq '{stackKey}' and ReadAt eq null";

        await foreach (var entity in tableClient.QueryAsync<NotificationEntity>(filter, cancellationToken: cancellationToken))
        {
            return NotificationMapper.ToDomain(entity);
        }

        return null;
    }

    public async Task AddAsync(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Cast to concrete type for mapping
        var concreteNotification = notification as Notification
            ?? throw new InvalidOperationException($"Expected {nameof(Notification)} but got {notification.GetType().Name}");

        var entity = NotificationMapper.ToEntity(concreteNotification);
        var tableClient = await GetTableClientAsync(cancellationToken);

        await tableClient.AddEntityAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Cast to concrete type for mapping
        var concreteNotification = notification as Notification
            ?? throw new InvalidOperationException($"Expected {nameof(Notification)} but got {notification.GetType().Name}");

        var entity = NotificationMapper.ToEntity(concreteNotification);
        var tableClient = await GetTableClientAsync(cancellationToken);

        try
        {
            await tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace, cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException($"Notification with ID '{notification.Id}' was not found.", ex);
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var tableClient = await GetTableClientAsync(cancellationToken);
        var filter = $"PartitionKey eq '{profileId}' and ReadAt eq null and DeletedAt eq null";

        var count = 0;
        await foreach (var _ in tableClient.QueryAsync<NotificationEntity>(
            filter,
            select: new[] { "RowKey" },
            cancellationToken: cancellationToken))
        {
            count++;
        }

        return count;
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdAsync(notificationId, cancellationToken);
        if (notification == null)
        {
            throw new InvalidOperationException($"Notification with ID '{notificationId}' was not found.");
        }

        notification.ReadAt = DateTime.UtcNow;
        await UpdateAsync(notification, cancellationToken);
    }

    public async Task MarkAsDeletedAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdAsync(notificationId, cancellationToken);
        if (notification == null)
        {
            throw new InvalidOperationException($"Notification with ID '{notificationId}' was not found.");
        }

        notification.DeletedAt = DateTime.UtcNow;
        await UpdateAsync(notification, cancellationToken);
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        var tableClient = await GetTableClientAsync(cancellationToken);
        var now = DateTime.UtcNow;

        // Query for expired notifications
        var filter = $"ExpiresAt lt datetime'{now:yyyy-MM-ddTHH:mm:ssZ}'";

        var entitiesToDelete = new List<NotificationEntity>();
        await foreach (var entity in tableClient.QueryAsync<NotificationEntity>(filter, cancellationToken: cancellationToken))
        {
            entitiesToDelete.Add(entity);
        }

        // Delete in batches
        foreach (var entity in entitiesToDelete)
        {
            await tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, cancellationToken: cancellationToken);
        }
    }

    private async Task<TableClient> GetTableClientAsync(CancellationToken cancellationToken)
    {
        var tableClient = _tableServiceClient.GetTableClient(TableName);
        await tableClient.CreateIfNotExistsAsync(cancellationToken);
        return tableClient;
    }
}
