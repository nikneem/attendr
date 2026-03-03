using System.Text.Json;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Data.Postgress.Entities;
using HexMaster.Attendr.Groups.Data.Postgress.Mappers;
using HexMaster.Attendr.Groups.DomainModels;
using Npgsql;

namespace HexMaster.Attendr.Groups.Data.Postgress;

/// <summary>
/// PostgreSQL implementation of ICheckInRepository using JSONB for storage.
/// </summary>
public sealed class PostgresCheckInRepository : ICheckInRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private const string TableName = "checkins";
    private readonly JsonSerializerOptions _jsonOptions;

    public PostgresCheckInRepository(NpgsqlDataSource dataSource)
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
    public async Task AddAsync(CheckIn checkIn, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkIn);

        var entity = CheckInMapper.ToEntity(checkIn);
        var dataJson = JsonSerializer.Serialize(entity, _jsonOptions);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            INSERT INTO {TableName} (id, group_id, conference_id, presentation_id, data, expiration)
            VALUES (@id, @group_id, @conference_id, @presentation_id, @data::jsonb, @expiration)";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", checkIn.Id);
        command.Parameters.AddWithValue("@group_id", checkIn.GroupId);
        command.Parameters.AddWithValue("@conference_id", checkIn.ConferenceId);
        command.Parameters.AddWithValue("@presentation_id", checkIn.PresentationId);
        command.Parameters.AddWithValue("@data", dataJson);
        command.Parameters.AddWithValue("@expiration", checkIn.Expiration.ToUniversalTime());

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddMemberAsync(Guid checkInId, CheckedInMember member, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(member);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // First, get the current check-in
        var checkIn = await GetByIdInternalAsync(connection, checkInId, cancellationToken).ConfigureAwait(false);
        if (checkIn == null)
        {
            throw new InvalidOperationException($"Check-in with ID {checkInId} not found.");
        }

        // Add the member
        checkIn.AddMember(member);

        // Update the check-in
        var entity = CheckInMapper.ToEntity(checkIn);
        var dataJson = JsonSerializer.Serialize(entity, _jsonOptions);

        var sql = $@"
            UPDATE {TableName}
            SET data = @data::jsonb
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", checkInId);
        command.Parameters.AddWithValue("@data", dataJson);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RemoveMemberAsync(Guid checkInId, Guid memberId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // First, get the current check-in
        var checkIn = await GetByIdInternalAsync(connection, checkInId, cancellationToken).ConfigureAwait(false);
        if (checkIn == null)
        {
            throw new InvalidOperationException($"Check-in with ID {checkInId} not found.");
        }

        // Remove the member
        checkIn.RemoveMember(memberId);

        // If no members left, delete the check-in
        if (!checkIn.Members.Any())
        {
            var deleteSql = $"DELETE FROM {TableName} WHERE id = @id";
            await using var deleteCommand = new NpgsqlCommand(deleteSql, connection);
            deleteCommand.Parameters.AddWithValue("@id", checkInId);
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        // Update the check-in
        var entity = CheckInMapper.ToEntity(checkIn);
        var dataJson = JsonSerializer.Serialize(entity, _jsonOptions);

        var sql = $@"
            UPDATE {TableName}
            SET data = @data::jsonb
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", checkInId);
        command.Parameters.AddWithValue("@data", dataJson);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CheckIn?> GetByConferenceAndPresentationAsync(Guid conferenceId, Guid presentationId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {TableName}
            WHERE conference_id = @conference_id 
              AND presentation_id = @presentation_id
              AND expiration > @now";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        command.Parameters.AddWithValue("@presentation_id", presentationId);
        command.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var dataJson = reader.GetString(0);
        var entity = JsonSerializer.Deserialize<CheckInEntity>(dataJson, _jsonOptions);

        return entity != null ? CheckInMapper.ToDomain(entity) : null;
    }

    /// <inheritdoc />
    public async Task<CheckIn?> GetByGroupConferenceAndPresentationAsync(Guid groupId, Guid conferenceId, Guid presentationId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {TableName}
            WHERE group_id = @group_id
              AND conference_id = @conference_id 
              AND presentation_id = @presentation_id
              AND expiration > @now";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@group_id", groupId);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        command.Parameters.AddWithValue("@presentation_id", presentationId);
        command.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var dataJson = reader.GetString(0);
        var entity = JsonSerializer.Deserialize<CheckInEntity>(dataJson, _jsonOptions);

        return entity != null ? CheckInMapper.ToDomain(entity) : null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CheckIn>> GetActiveByConferenceAsync(Guid conferenceId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {TableName}
            WHERE conference_id = @conference_id
              AND expiration > @now
            ORDER BY (data->'presentationData'->>'startDateTime')::timestamp";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        command.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow);

        var checkIns = new List<CheckIn>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var dataJson = reader.GetString(0);
            var entity = JsonSerializer.Deserialize<CheckInEntity>(dataJson, _jsonOptions);

            if (entity != null)
            {
                checkIns.Add(CheckInMapper.ToDomain(entity));
            }
        }

        return checkIns.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CheckIn>> GetActiveByGroupAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            SELECT data
            FROM {TableName}
            WHERE group_id = @group_id
              AND expiration > @now
            ORDER BY expiration DESC";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@group_id", groupId);
        command.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow);

        var checkIns = new List<CheckIn>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var dataJson = reader.GetString(0);
            var entity = JsonSerializer.Deserialize<CheckInEntity>(dataJson, _jsonOptions);

            if (entity != null)
            {
                checkIns.Add(CheckInMapper.ToDomain(entity));
            }
        }

        return checkIns.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = $@"
            DELETE FROM {TableName}
            WHERE expiration <= @now";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rowsAffected;
    }

    private async Task<CheckIn?> GetByIdInternalAsync(NpgsqlConnection connection, Guid id, CancellationToken cancellationToken)
    {
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
        var entity = JsonSerializer.Deserialize<CheckInEntity>(dataJson, _jsonOptions);

        return entity != null ? CheckInMapper.ToDomain(entity) : null;
    }

    private async Task EnsureTableExistsAsync()
    {
        await using var connection = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);

        var createTableSql = $@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                id UUID PRIMARY KEY,
                group_id UUID NOT NULL,
                conference_id UUID NOT NULL,
                presentation_id UUID NOT NULL,
                data JSONB NOT NULL,
                expiration TIMESTAMPTZ NOT NULL,
                CONSTRAINT unique_group_conference_presentation UNIQUE (group_id, conference_id, presentation_id)
            )";

        await using (var command = new NpgsqlCommand(createTableSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // Create indexes for better query performance
        var createIndexesSql = $@"
            CREATE INDEX IF NOT EXISTS idx_{TableName}_group_id ON {TableName}(group_id);
            CREATE INDEX IF NOT EXISTS idx_{TableName}_conference_id ON {TableName}(conference_id);
            CREATE INDEX IF NOT EXISTS idx_{TableName}_presentation_id ON {TableName}(presentation_id);
            CREATE INDEX IF NOT EXISTS idx_{TableName}_expiration ON {TableName}(expiration);
            CREATE INDEX IF NOT EXISTS idx_{TableName}_group_conference ON {TableName}(group_id, conference_id);
        ";

        await using (var command = new NpgsqlCommand(createIndexesSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
