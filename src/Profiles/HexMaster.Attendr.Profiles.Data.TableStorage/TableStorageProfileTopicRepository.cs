using Azure;
using Azure.Data.Tables;
using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Profiles.Data.TableStorage.Mappers;
using HexMaster.Attendr.Profiles.Data.TableStorage.Models;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Repositories;

namespace HexMaster.Attendr.Profiles.Data.TableStorage;

public sealed class TableStorageProfileTopicRepository(TableServiceClient tableServiceClient) : IProfileTopicRepository
{
    public async Task<IReadOnlyList<ProfileTopic>> GetByProfileIdAsync(string profileId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);

        var normalizedProfileId = profileId.Trim();
        var tableClient = await GetTableClient(cancellationToken);
        var filter = $"PartitionKey eq '{normalizedProfileId}'";

        var topics = new List<ProfileTopic>();
        await foreach (var entity in tableClient.QueryAsync<ProfileTopicEntity>(filter, cancellationToken: cancellationToken))
        {
            topics.Add(ProfileTopicMapper.ToDomain(entity));
        }

        return topics;
    }

    public async Task<ProfileTopic?> GetByProfileIdAndKeyAsync(string profileId, string topicKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        ArgumentException.ThrowIfNullOrWhiteSpace(topicKey);

        var normalizedProfileId = profileId.Trim();
        var normalizedKey = topicKey.Trim().ToLowerInvariant();
        var tableClient = await GetTableClient(cancellationToken);

        try
        {
            var entity = await tableClient.GetEntityAsync<ProfileTopicEntity>(normalizedProfileId, normalizedKey, cancellationToken: cancellationToken);
            return ProfileTopicMapper.ToDomain(entity.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task UpsertAsync(ProfileTopic topic, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(topic);

        var entity = ProfileTopicMapper.ToEntity(topic);
        var tableClient = await GetTableClient(cancellationToken);

        await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken);
    }

    private async Task<TableClient> GetTableClient(CancellationToken cancellationToken)
    {
        var tableClient = tableServiceClient.GetTableClient(AspireConstants.TableStorage.ProfileTopics);
        await tableClient.CreateIfNotExistsAsync(cancellationToken);
        return tableClient;
    }
}
