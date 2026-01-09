using System.Text.Json;
using HexMaster.Attendr.Presence.Data.Postgres.Entities;
using HexMaster.Attendr.Presence.Data.Postgres.Mappers;
using HexMaster.Attendr.Presence.DomainModels;
using Npgsql;

namespace HexMaster.Attendr.Presence.Data.Postgres;

/// <summary>
/// PostgreSQL implementation of IConferencePresenceRepository using JSONB storage.
/// </summary>
public sealed class PostgresConferencePresenceRepository : IConferencePresenceRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private const string TableName = "conference_presence";
    private readonly JsonSerializerOptions _jsonOptions;

    public PostgresConferencePresenceRepository(NpgsqlDataSource dataSource)
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
    public async Task<bool> ExistsAsync(Guid profileId, Guid conferenceId, CancellationToken cancellationToken = default)
    {
        var id = ConferencePresenceMapper.BuildId(profileId, conferenceId);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT COUNT(*)
            FROM {TableName}
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        return count > 0;
    }

    /// <inheritdoc />
    public async Task AddAsync(ConferencePresence presence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(presence);

        var id = ConferencePresenceMapper.BuildId(presence.ProfileId, presence.ConferenceId);
        var entity = ConferencePresenceMapper.ToEntity(presence);
        var dataJson = JsonSerializer.Serialize(entity, _jsonOptions);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            INSERT INTO {TableName} (id, profile_id, conference_id, data)
            VALUES (@id, @profile_id, @conference_id, @data::jsonb)";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@profile_id", presence.ProfileId);
        command.Parameters.AddWithValue("@conference_id", presence.ConferenceId);
        command.Parameters.AddWithValue("@data", dataJson);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ConferencePresence>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {TableName}
            WHERE profile_id = @profile_id
            ORDER BY (data->>'startDate')::date";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@profile_id", profileId);

        var presences = new List<ConferencePresence>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var dataJson = reader.GetString(0);
            var entity = JsonSerializer.Deserialize<ConferencePresenceEntity>(dataJson, _jsonOptions);
            
            if (entity != null)
            {
                presences.Add(ConferencePresenceMapper.ToDomain(entity));
            }
        }

        return presences.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<ConferencePresence?> GetAsync(Guid conferenceId, Guid profileId, CancellationToken cancellationToken = default)
    {
        var id = ConferencePresenceMapper.BuildId(profileId, conferenceId);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {TableName}
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var dataJson = reader.GetString(0);
        var entity = JsonSerializer.Deserialize<ConferencePresenceEntity>(dataJson, _jsonOptions);

        return entity != null ? ConferencePresenceMapper.ToDomain(entity) : null;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ConferencePresence presence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(presence);

        var id = ConferencePresenceMapper.BuildId(presence.ProfileId, presence.ConferenceId);
        var entity = ConferencePresenceMapper.ToEntity(presence);
        var dataJson = JsonSerializer.Serialize(entity, _jsonOptions);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            UPDATE {TableName}
            SET data = @data::jsonb
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@data", dataJson);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Conference presence with ID '{id}' was not found.");
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid conferenceId, Guid profileId, CancellationToken cancellationToken = default)
    {
        var id = ConferencePresenceMapper.BuildId(profileId, conferenceId);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            DELETE FROM {TableName}
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureTableExistsAsync()
    {
        await using var connection = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);

        var createTableSql = $@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                id VARCHAR(100) PRIMARY KEY,
                profile_id UUID NOT NULL,
                conference_id UUID NOT NULL,
                data JSONB NOT NULL,
                CONSTRAINT unique_profile_conference UNIQUE (profile_id, conference_id)
            )";

        await using (var command = new NpgsqlCommand(createTableSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // Create indexes for better query performance
        var createIndexesSql = $@"
            CREATE INDEX IF NOT EXISTS idx_{TableName}_profile_id ON {TableName}(profile_id);
            CREATE INDEX IF NOT EXISTS idx_{TableName}_conference_id ON {TableName}(conference_id);
            CREATE INDEX IF NOT EXISTS idx_{TableName}_data_start_date ON {TableName}((data->>'startDate'));
        ";

        await using (var command = new NpgsqlCommand(createIndexesSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
