using Azure;
using Azure.Data.Tables;
using HexMaster.Attendr.Profiles.Data.TableStorage.Mappers;
using HexMaster.Attendr.Profiles.Data.TableStorage.Models;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Repositories;
using HexMaster.Attendr.Aspire.AppHost;

namespace HexMaster.Attendr.Profiles.Data.TableStorage;

/// <summary>
/// Azure Table Storage implementation of IProfileRepository.
/// </summary>
public sealed class TableStorageProfileRepository(TableServiceClient tableServiceClient) : IProfileRepository
{


    /// <inheritdoc />
    public async Task<Profile?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        // We need to query across all partitions to find by ID (RowKey)
        // This is less efficient than GetBySubjectIdAsync
        var filter = $"RowKey eq '{id}'";

        var tableClient = await GetTableClient(cancellationToken);
        await foreach (var entity in tableClient.QueryAsync<ProfileEntity>(filter, cancellationToken: cancellationToken))
        {
            return ProfileMapper.ToDomain(entity);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<Profile?> GetBySubjectIdAsync(string subjectId, CancellationToken cancellationToken = default)
    {
        // Query by PartitionKey for efficient lookup
        var filter = $"PartitionKey eq '{subjectId}'";

        var tableClient = await GetTableClient(cancellationToken);
        await foreach (var entity in tableClient.QueryAsync<ProfileEntity>(filter, cancellationToken: cancellationToken))
        {
            return ProfileMapper.ToDomain(entity);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task AddAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var entity = ProfileMapper.ToEntity(profile);
        var tableClient = await GetTableClient(cancellationToken);
        await tableClient.AddEntityAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var entity = ProfileMapper.ToEntity(profile);

        try
        {
            var tableClient = await GetTableClient(cancellationToken);
            await tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace, cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException($"Profile with ID '{profile.Id}' was not found.", ex);
        }
    }

    private async Task<TableClient> GetTableClient(CancellationToken cancellationToken)
    {
        var tableClient = tableServiceClient.GetTableClient(AspireConstants.TableStorage.Profiles);
        await tableClient.CreateIfNotExistsAsync(cancellationToken);

        return tableClient;
    }
}
