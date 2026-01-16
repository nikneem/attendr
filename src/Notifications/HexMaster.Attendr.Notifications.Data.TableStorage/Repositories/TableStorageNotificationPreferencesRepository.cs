using Azure;
using Azure.Data.Tables;
using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.Data.TableStorage.Entities;
using HexMaster.Attendr.Notifications.Data.TableStorage.Mappers;
using HexMaster.Attendr.Notifications.DomainModels;

namespace HexMaster.Attendr.Notifications.Data.TableStorage.Repositories;

/// <summary>
/// Azure Table Storage implementation of INotificationPreferencesRepository.
/// </summary>
public sealed class TableStorageNotificationPreferencesRepository : INotificationPreferencesRepository
{
    private const string TableName = AspireConstants.TableStorage.NotificationPreferences;
    private readonly TableServiceClient _tableServiceClient;

    public TableStorageNotificationPreferencesRepository(TableServiceClient tableServiceClient)
    {
        _tableServiceClient = tableServiceClient ?? throw new ArgumentNullException(nameof(tableServiceClient));
    }

    public async Task<INotificationPreferences?> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var tableClient = await GetTableClientAsync(cancellationToken);

        try
        {
            var response = await tableClient.GetEntityAsync<NotificationPreferencesEntity>(
                profileId.ToString(),
                NotificationPreferencesEntity.PreferencesRowKey,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return NotificationPreferencesMapper.ToDomain(response.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task UpsertAsync(INotificationPreferences preferences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        // Cast to concrete type for mapping
        var concretePreferences = preferences as NotificationPreferences
            ?? throw new InvalidOperationException($"Expected {nameof(NotificationPreferences)} but got {preferences.GetType().Name}");

        var entity = NotificationPreferencesMapper.ToEntity(concretePreferences);
        var tableClient = await GetTableClientAsync(cancellationToken);

        await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateTypeChannelPreferencesAsync(
        Guid profileId,
        string typeKey,
        Dictionary<NotificationChannel, bool> channelSettings,
        CancellationToken cancellationToken = default)
    {
        var preferences = await GetByProfileIdAsync(profileId, cancellationToken);

        if (preferences == null)
        {
            // Create new preferences with this single type setting
            preferences = new NotificationPreferences
            {
                ProfileId = profileId,
                TypeChannelPreferences = new Dictionary<string, Dictionary<NotificationChannel, bool>>
                {
                    [typeKey] = channelSettings
                },
                CreatedAt = DateTime.UtcNow
            };
        }
        else
        {
            // Update existing preferences
            preferences.TypeChannelPreferences[typeKey] = channelSettings;
            preferences.UpdatedAt = DateTime.UtcNow;
        }

        await UpsertAsync(preferences, cancellationToken);
    }

    public async Task UpdateDoNotDisturbAsync(
        Guid profileId,
        DateTime? doNotDisturbUntil,
        CancellationToken cancellationToken = default)
    {
        var preferences = await GetByProfileIdAsync(profileId, cancellationToken);

        if (preferences == null)
        {
            // Create new preferences with DND setting
            preferences = new NotificationPreferences
            {
                ProfileId = profileId,
                TypeChannelPreferences = new Dictionary<string, Dictionary<NotificationChannel, bool>>(),
                DoNotDisturbUntil = doNotDisturbUntil,
                CreatedAt = DateTime.UtcNow
            };
        }
        else
        {
            preferences.DoNotDisturbUntil = doNotDisturbUntil;
            preferences.UpdatedAt = DateTime.UtcNow;
        }

        await UpsertAsync(preferences, cancellationToken);
    }

    private async Task<TableClient> GetTableClientAsync(CancellationToken cancellationToken)
    {
        var tableClient = _tableServiceClient.GetTableClient(TableName);
        await tableClient.CreateIfNotExistsAsync(cancellationToken);
        return tableClient;
    }
}
