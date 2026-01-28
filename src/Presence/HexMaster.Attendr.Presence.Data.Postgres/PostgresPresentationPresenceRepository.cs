using System.Text.Json;
using HexMaster.Attendr.Presence.Data.Postgres.Entities;
using HexMaster.Attendr.Presence.Data.Postgres.Mappers;
using HexMaster.Attendr.Presence.DomainModels;
using Npgsql;

namespace HexMaster.Attendr.Presence.Data.Postgres;

/// <summary>
/// PostgreSQL implementation of IPresentationPresenceRepository using JSONB storage.
/// </summary>
public sealed class PostgresPresentationPresenceRepository : IPresentationPresenceRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private const string TableName = "presentation_presence";
    private readonly JsonSerializerOptions _jsonOptions;

    public PostgresPresentationPresenceRepository(NpgsqlDataSource dataSource)
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
    public async Task AddAsync(PresentationPresence presentation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        var id = PresentationPresenceMapper.BuildId(presentation.ProfileId, presentation.ConferenceId, presentation.PresentationId);
        var entity = PresentationPresenceMapper.ToEntity(presentation);
        var dataJson = JsonSerializer.Serialize(entity, _jsonOptions);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            INSERT INTO {TableName} (id, profile_id, conference_id, presentation_id, data)
            VALUES (@id, @profile_id, @conference_id, @presentation_id, @data::jsonb)";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@profile_id", presentation.ProfileId);
        command.Parameters.AddWithValue("@conference_id", presentation.ConferenceId);
        command.Parameters.AddWithValue("@presentation_id", presentation.PresentationId);
        command.Parameters.AddWithValue("@data", dataJson);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddManyAsync(IEnumerable<PresentationPresence> presentations, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(presentations);

        var presentationList = presentations.ToList();
        if (presentationList.Count == 0)
        {
            return;
        }

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            foreach (var presentation in presentationList)
            {
                var id = PresentationPresenceMapper.BuildId(presentation.ProfileId, presentation.ConferenceId, presentation.PresentationId);
                var entity = PresentationPresenceMapper.ToEntity(presentation);
                var dataJson = JsonSerializer.Serialize(entity, _jsonOptions);

                var sql = $@"
                    INSERT INTO {TableName} (id, profile_id, conference_id, presentation_id, data)
                    VALUES (@id, @profile_id, @conference_id, @presentation_id, @data::jsonb)";

                await using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@profile_id", presentation.ProfileId);
                command.Parameters.AddWithValue("@conference_id", presentation.ConferenceId);
                command.Parameters.AddWithValue("@presentation_id", presentation.PresentationId);
                command.Parameters.AddWithValue("@data", dataJson);

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<PresentationPresence>> GetByConferenceAndPresentationAsync(
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {TableName}
            WHERE conference_id = @conference_id AND presentation_id = @presentation_id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        command.Parameters.AddWithValue("@presentation_id", presentationId);

        var presentations = new List<PresentationPresence>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var dataJson = reader.GetString(0);
            var entity = JsonSerializer.Deserialize<PresentationPresenceEntity>(dataJson, _jsonOptions);

            if (entity != null)
            {
                presentations.Add(PresentationPresenceMapper.ToDomain(entity));
            }
        }

        return presentations.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<PresentationPresence?> GetByIdAsync(
        Guid profileId,
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default)
    {
        var id = PresentationPresenceMapper.BuildId(profileId, conferenceId, presentationId);

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
        var entity = JsonSerializer.Deserialize<PresentationPresenceEntity>(dataJson, _jsonOptions);

        return entity != null ? PresentationPresenceMapper.ToDomain(entity) : null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<PresentationPresence>> GetUnratedByProfileAndConferenceAsync(
        Guid profileId,
        Guid conferenceId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {TableName}
            WHERE profile_id = @profile_id 
              AND conference_id = @conference_id
              AND (data->>'isRated')::boolean = false
            ORDER BY (data->>'startDateTime')::timestamp";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@profile_id", profileId);
        command.Parameters.AddWithValue("@conference_id", conferenceId);

        var presentations = new List<PresentationPresence>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var dataJson = reader.GetString(0);
            var entity = JsonSerializer.Deserialize<PresentationPresenceEntity>(dataJson, _jsonOptions);

            if (entity != null)
            {
                presentations.Add(PresentationPresenceMapper.ToDomain(entity));
            }
        }

        return presentations.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<PresentationPresence>> GetByProfileAndConferenceAsync(
        Guid profileId,
        Guid conferenceId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {TableName}
            WHERE profile_id = @profile_id AND conference_id = @conference_id
            ORDER BY (data->>'startDateTime')::timestamp";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@profile_id", profileId);
        command.Parameters.AddWithValue("@conference_id", conferenceId);

        var presentations = new List<PresentationPresence>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var dataJson = reader.GetString(0);
            var entity = JsonSerializer.Deserialize<PresentationPresenceEntity>(dataJson, _jsonOptions);

            if (entity != null)
            {
                presentations.Add(PresentationPresenceMapper.ToDomain(entity));
            }
        }

        return presentations.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<PresentationPresence>> GetByProfileAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {TableName}
            WHERE profile_id = @profile_id
            ORDER BY (data->>'startDateTime')::timestamp";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@profile_id", profileId);

        var presentations = new List<PresentationPresence>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var dataJson = reader.GetString(0);
            var entity = JsonSerializer.Deserialize<PresentationPresenceEntity>(dataJson, _jsonOptions);

            if (entity != null)
            {
                presentations.Add(PresentationPresenceMapper.ToDomain(entity));
            }
        }

        return presentations.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(
        Guid profileId,
        Guid conferenceId,
        PresentationPresence presentation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        var id = PresentationPresenceMapper.BuildId(profileId, conferenceId, presentation.PresentationId);
        var entity = PresentationPresenceMapper.ToEntity(presentation);
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
            throw new InvalidOperationException($"Presentation presence with ID '{id}' was not found.");
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        Guid profileId,
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default)
    {
        var id = PresentationPresenceMapper.BuildId(profileId, conferenceId, presentationId);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            DELETE FROM {TableName}
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> ResetRatingsAsync(
        Guid profileId,
        Guid conferenceId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            UPDATE {TableName}
            SET data = jsonb_set(
                jsonb_set(
                    jsonb_set(data, '{{isRated}}', 'false'),
                    '{{isFavorite}}', 'false'),
                '{{rating}}', 'null')
            WHERE profile_id = @profile_id 
                AND conference_id = @conference_id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@profile_id", profileId);
        command.Parameters.AddWithValue("@conference_id", conferenceId);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rowsAffected;
    }

    private async Task EnsureTableExistsAsync()
    {
        await using var connection = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);

        var createTableSql = $@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                id VARCHAR(150) PRIMARY KEY,
                profile_id UUID NOT NULL,
                conference_id UUID NOT NULL,
                presentation_id UUID NOT NULL,
                data JSONB NOT NULL,
                CONSTRAINT fk_conference_presence FOREIGN KEY (profile_id, conference_id) 
                    REFERENCES conference_presence(profile_id, conference_id) ON DELETE CASCADE
            )";

        await using (var command = new NpgsqlCommand(createTableSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // Create indexes for better query performance
        var createIndexesSql = $@"
            CREATE INDEX IF NOT EXISTS idx_{TableName}_profile_conference ON {TableName}(profile_id, conference_id);
            CREATE INDEX IF NOT EXISTS idx_{TableName}_conference_presentation ON {TableName}(conference_id, presentation_id);
            CREATE INDEX IF NOT EXISTS idx_{TableName}_data_is_rated ON {TableName}((data->>'isRated'));
            CREATE INDEX IF NOT EXISTS idx_{TableName}_data_start_date_time ON {TableName}((data->>'startDateTime'));
        ";

        await using (var command = new NpgsqlCommand(createIndexesSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
