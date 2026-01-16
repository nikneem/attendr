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
/// Azure Table Storage implementation of IPushSubscriptionRepository.
/// </summary>
public sealed class TableStoragePushSubscriptionRepository : IPushSubscriptionRepository
{
    private const string TableName = AspireConstants.TableStorage.Subscriptions;
    private readonly TableServiceClient _tableServiceClient;

    public TableStoragePushSubscriptionRepository(TableServiceClient tableServiceClient)
    {
        _tableServiceClient = tableServiceClient ?? throw new ArgumentNullException(nameof(tableServiceClient));
    }

    public async Task UpsertAsync(IPushSubscription subscription, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);

        var concrete = subscription as PushSubscription
            ?? throw new InvalidOperationException($"Expected {nameof(PushSubscription)} but got {subscription.GetType().Name}");

        var entity = PushSubscriptionMapper.ToEntity(concrete);
        var tableClient = await GetTableClientAsync(cancellationToken);

        await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<IPushSubscription>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var tableClient = await GetTableClientAsync(cancellationToken);
        var filter = $"PartitionKey eq '{profileId}'";

        var subscriptions = new List<PushSubscription>();
        await foreach (var entity in tableClient.QueryAsync<PushSubscriptionEntity>(filter, cancellationToken: cancellationToken))
        {
            subscriptions.Add(PushSubscriptionMapper.ToDomain(entity));
        }

        return subscriptions;
    }

    public async Task DeleteAsync(Guid profileId, string endpoint, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);

        var tableClient = await GetTableClientAsync(cancellationToken);
        var rowKey = PushSubscriptionEntity.CreateRowKey(endpoint);

        try
        {
            await tableClient.DeleteEntityAsync(profileId.ToString(), rowKey, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Already deleted / not found; ignore
        }
    }

    private async Task<TableClient> GetTableClientAsync(CancellationToken cancellationToken)
    {
        var tableClient = _tableServiceClient.GetTableClient(TableName);
        await tableClient.CreateIfNotExistsAsync(cancellationToken);
        return tableClient;
    }
}
