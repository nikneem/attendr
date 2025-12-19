using System.Reflection;
using Azure;
using Azure.Data.Tables;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Repositories;

namespace HexMaster.Attendr.Profiles.Data.TableStorage;

/// <summary>
/// Azure Table Storage backed repository for profiles.
/// </summary>
public sealed class TableStorageProfileRepository : IProfileRepository
{
    private const string Partition = "user";
    private readonly TableClient _tableClient;

    public TableStorageProfileRepository(TableClient tableClient)
    {
        _tableClient = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
    }

    public async Task AddAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var entity = ToEntity(profile);
        await _tableClient.AddEntityAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Profile?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        try
        {
            var response = await _tableClient.GetEntityAsync<ProfileTableEntity>(Partition, id, cancellationToken: cancellationToken).ConfigureAwait(false);
            return ToDomain(response.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<Profile?> GetBySubjectIdAsync(string subjectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return null;
        }

        var sanitizedSubject = subjectId.Replace("'", "''", StringComparison.Ordinal);
        var filter = $"PartitionKey eq '{Partition}' and SubjectId eq '{sanitizedSubject}'";

        await foreach (var entity in _tableClient.QueryAsync<ProfileTableEntity>(filter: filter, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            return ToDomain(entity);
        }

        return null;
    }

    public async Task UpdateAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var entity = ToEntity(profile);
        await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken).ConfigureAwait(false);
    }

    private static ProfileTableEntity ToEntity(Profile profile)
    {
        return new ProfileTableEntity
        {
            PartitionKey = Partition,
            RowKey = profile.Id,
            SubjectId = profile.SubjectId,
            DisplayName = profile.DisplayName,
            FirstName = profile.FirstName ?? string.Empty,
            LastName = profile.LastName ?? string.Empty,
            Email = profile.Email,
            Employee = profile.Employee,
            TagLine = profile.TagLine,
            Enabled = profile.Enabled,
            IsSearchable = profile.IsSearchable,
            CreatedOn = profile.CreatedOn,
            ModifiedOn = profile.ModifiedOn
        };
    }

    private static Profile ToDomain(ProfileTableEntity entity)
    {
        var profile = new Profile(
            entity.RowKey,
            entity.SubjectId,
            entity.DisplayName,
            entity.FirstName,
            entity.LastName,
            entity.Email,
            entity.Employee,
            entity.TagLine,
            entity.Enabled,
            entity.IsSearchable);

        ApplyAudit(profile, entity.CreatedOn, entity.ModifiedOn);
        return profile;
    }

    private static void ApplyAudit(Profile profile, DateTimeOffset createdOn, DateTimeOffset? modifiedOn)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setCreated = typeof(Profile).GetMethod("SetCreatedOn", flags);
        setCreated?.Invoke(profile, new object[] { createdOn });

        var setModified = typeof(Profile).GetMethod("SetModifiedOn", flags);
        setModified?.Invoke(profile, new object?[] { modifiedOn });
    }
}
