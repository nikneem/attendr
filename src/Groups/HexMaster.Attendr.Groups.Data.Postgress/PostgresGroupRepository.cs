using System.Text.Json;
using HexMaster.Attendr.Groups.Data.Postgress.Entities;
using HexMaster.Attendr.Groups.Data.Postgress.Mappers;
using HexMaster.Attendr.Groups.DomainModels;
using Npgsql;

namespace HexMaster.Attendr.Groups.Data.Postgress;

/// <summary>
/// PostgreSQL implementation of IGroupRepository using JSONB for storage.
/// </summary>
public sealed class PostgresGroupRepository : IGroupRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private const string GroupsTableName = "groups";
    private readonly JsonSerializerOptions _jsonOptions;

    public PostgresGroupRepository(NpgsqlDataSource dataSource)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        EnsureTableExistsAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task AddAsync(Group group, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        var entity = GroupMapper.ToEntity(group);
        var dataJson = JsonSerializer.Serialize(entity, _jsonOptions);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            INSERT INTO {GroupsTableName} (id, name, data)
            VALUES (@id, @name, @data::jsonb)";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", entity.Id);
        command.Parameters.AddWithValue("name", entity.Name);
        command.Parameters.AddWithValue("data", dataJson);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Group group, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        var entity = GroupMapper.ToEntity(group);
        var dataJson = JsonSerializer.Serialize(entity, _jsonOptions);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            UPDATE {GroupsTableName}
            SET name = @name, data = @data::jsonb
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", entity.Id);
        command.Parameters.AddWithValue("name", entity.Name);
        command.Parameters.AddWithValue("data", dataJson);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Group with ID '{group.Id}' was not found.");
        }
    }

    /// <inheritdoc />
    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {GroupsTableName}
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var dataJson = reader.GetString(0);
        var entity = JsonSerializer.Deserialize<GroupEntity>(dataJson, _jsonOptions);

        return entity != null ? GroupMapper.ToDomain(entity) : null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Group>> GetGroupsByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {GroupsTableName}
            WHERE data->'members' @> @memberJson::jsonb";

        await using var command = new NpgsqlCommand(sql, connection);

        // Create a JSON fragment to match members with the given ID
        var memberJson = JsonSerializer.Serialize(new[] { new { id = memberId } }, _jsonOptions);
        command.Parameters.AddWithValue("memberJson", memberJson);

        var groups = new List<Group>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var dataJson = reader.GetString(0);
            var entity = JsonSerializer.Deserialize<GroupEntity>(dataJson, _jsonOptions);

            if (entity != null)
            {
                groups.Add(GroupMapper.ToDomain(entity));
            }
        }

        return groups.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<Group> Groups, int TotalCount)> ListGroupsAsync(
        string? searchQuery,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // Build the WHERE clause
        var whereClause = "data->'settings'->>'isSearchable' = 'true'";

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            whereClause += " AND name ILIKE @searchQuery";
        }

        // Get total count
        var countSql = $@"
            SELECT COUNT(*)
            FROM {GroupsTableName}
            WHERE {whereClause}";

        await using var countCommand = new NpgsqlCommand(countSql, connection);
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            countCommand.Parameters.AddWithValue("searchQuery", $"%{searchQuery}%");
        }

        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));

        // Get paginated results
        var dataSql = $@"
            SELECT data
            FROM {GroupsTableName}
            WHERE {whereClause}
            ORDER BY name
            LIMIT @pageSize OFFSET @offset";

        await using var dataCommand = new NpgsqlCommand(dataSql, connection);
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            dataCommand.Parameters.AddWithValue("searchQuery", $"%{searchQuery}%");
        }
        dataCommand.Parameters.AddWithValue("pageSize", pageSize);
        dataCommand.Parameters.AddWithValue("offset", (pageNumber - 1) * pageSize);

        var groups = new List<Group>();
        await using var reader = await dataCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var dataJson = reader.GetString(0);
            var entity = JsonSerializer.Deserialize<GroupEntity>(dataJson, _jsonOptions);

            if (entity != null)
            {
                groups.Add(GroupMapper.ToDomain(entity));
            }
        }

        return (groups.AsReadOnly(), totalCount);
    }

    private async Task EnsureTableExistsAsync()
    {
        await using var connection = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);

        // Create table if it doesn't exist
        var createTableSql = $@"
            CREATE TABLE IF NOT EXISTS {GroupsTableName} (
                id UUID PRIMARY KEY,
                name TEXT NOT NULL,
                data JSONB NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
            )";

        await using (var tableCommand = new NpgsqlCommand(createTableSql, connection))
        {
            await tableCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // Create indexes for better query performance
        var createIndexSql = $@"
            CREATE INDEX IF NOT EXISTS idx_{GroupsTableName}_name 
            ON {GroupsTableName} (name);
            
            CREATE INDEX IF NOT EXISTS idx_{GroupsTableName}_searchable 
            ON {GroupsTableName} ((data->'settings'->>'isSearchable'));
            
            CREATE INDEX IF NOT EXISTS idx_{GroupsTableName}_members 
            ON {GroupsTableName} USING GIN ((data->'members'))";

        await using (var indexCommand = new NpgsqlCommand(createIndexSql, connection))
        {
            await indexCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
