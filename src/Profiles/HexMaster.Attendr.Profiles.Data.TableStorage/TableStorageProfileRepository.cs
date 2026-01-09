using Azure;
using Azure.Data.Tables;
using HexMaster.Attendr.Profiles.Data.TableStorage.Mappers;
using HexMaster.Attendr.Profiles.Data.TableStorage.Models;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Repositories;

namespace HexMaster.Attendr.Profiles.Data.TableStorage;

/// <summary>
/// Azure Table Storage implementation of IProfileRepository.
/// </summary>
public sealed class TableStorageProfileRepository : IProfileRepository
{
    private readonly TableClient _tableClient;

    public TableStorageProfileRepository(TableServiceClient tableServiceClient, TableStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(tableServiceClient);
        ArgumentNullException.ThrowIfNull(options);
        
        _tableClient = tableServiceClient.GetTableClient(options.TableName);
        _tableClient.CreateIfNotExists();
    }

    /// <inheritdoc />
    public async Task<Profile?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        // We need to query across all partitions to find by ID (RowKey)
        // This is less efficient than GetBySubjectIdAsync
        var filter = $"RowKey eq '{id}'";
        
        await foreach (var entity in _tableClient.QueryAsync<ProfileEntity>(filter, cancellationToken: cancellationToken))
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
        
        await foreach (var entity in _tableClient.QueryAsync<ProfileEntity>(filter, cancellationToken: cancellationToken))
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
        await _tableClient.AddEntityAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var entity = ProfileMapper.ToEntity(profile);
        
        try
        {
            await _tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace, cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException($"Profile with ID '{profile.Id}' was not found.", ex);
        }
    }
}
