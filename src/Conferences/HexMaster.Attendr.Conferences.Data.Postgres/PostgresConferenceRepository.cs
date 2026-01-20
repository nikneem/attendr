using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Data.Postgres.Entities;
using HexMaster.Attendr.Conferences.Data.Postgres.Mappers;
using HexMaster.Attendr.Conferences.DomainModels;
using Npgsql;

namespace HexMaster.Attendr.Conferences.Data.Postgres;

/// <summary>
/// PostgreSQL implementation of IConferenceRepository using fully relational database structure.
/// </summary>
public sealed class PostgresConferenceRepository : IConferenceRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresConferenceRepository(NpgsqlDataSource dataSource)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;

        EnsureTablesExistAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task AddAsync(Conference conference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conference);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Insert conference
            var conferenceEntity = ConferenceMapper.ToEntity(conference);
            await InsertConferenceAsync(connection, conferenceEntity, cancellationToken).ConfigureAwait(false);

            // Insert rooms
            foreach (var room in conference.Rooms)
            {
                await InsertRoomAsync(connection, conference.Id, room, cancellationToken).ConfigureAwait(false);
            }

            // Insert speakers
            foreach (var speaker in conference.Speakers)
            {
                await InsertSpeakerAsync(connection, conference.Id, speaker, cancellationToken).ConfigureAwait(false);
            }

            // Insert presentations and their speaker relationships
            foreach (var presentation in conference.Presentations)
            {
                await InsertPresentationAsync(connection, conference.Id, presentation, cancellationToken).ConfigureAwait(false);

                foreach (var speakerId in presentation.SpeakerIds)
                {
                    await InsertPresentationSpeakerAsync(connection, presentation.Id, speakerId, cancellationToken).ConfigureAwait(false);
                }
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
    public async Task<Conference?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // Load conference
        var conferenceEntity = await LoadConferenceAsync(connection, id, cancellationToken).ConfigureAwait(false);
        if (conferenceEntity == null)
        {
            return null;
        }

        // Load related entities
        var rooms = await LoadRoomsAsync(connection, id, cancellationToken).ConfigureAwait(false);
        var speakers = await LoadSpeakersAsync(connection, id, cancellationToken).ConfigureAwait(false);
        var presentations = await LoadPresentationsAsync(connection, id, cancellationToken).ConfigureAwait(false);
        var presentationSpeakers = await LoadPresentationSpeakersAsync(connection, id, cancellationToken).ConfigureAwait(false);

        return ConferenceMapper.ToDomain(conferenceEntity, rooms, speakers, presentations, presentationSpeakers);
    }

    /// <inheritdoc />
    public async Task<ConferenceDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // Load conference
        var conferenceEntity = await LoadConferenceAsync(connection, id, cancellationToken).ConfigureAwait(false);
        if (conferenceEntity == null)
        {
            return null;
        }

        // Load related entities
        var rooms = await LoadRoomsAsync(connection, id, cancellationToken).ConfigureAwait(false);
        var speakers = await LoadSpeakersAsync(connection, id, cancellationToken).ConfigureAwait(false);
        var presentations = await LoadPresentationsAsync(connection, id, cancellationToken).ConfigureAwait(false);
        var presentationSpeakers = await LoadPresentationSpeakersAsync(connection, id, cancellationToken).ConfigureAwait(false);

        return ConferenceDtoMapper.ToDetailsDto(conferenceEntity, rooms, speakers, presentations, presentationSpeakers);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Conference conference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conference);

        // If the conference hasn't been modified, skip the update
        if (conference.State == HexMaster.Attendr.Core.DomainModels.DomainModelState.Pristine)
        {
            return;
        }

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Update conference only if it was actually modified (not just touched)
            if (conference.State == HexMaster.Attendr.Core.DomainModels.DomainModelState.Modified)
            {
                var conferenceEntity = ConferenceMapper.ToEntity(conference);
                await UpdateConferenceAsync(connection, conferenceEntity, cancellationToken).ConfigureAwait(false);
            }

            // Update rooms: only delete and re-insert if any room was created or modified (not just touched)
            var modifiedRooms = conference.Rooms.Where(r => 
                r.State == HexMaster.Attendr.Core.DomainModels.DomainModelState.Created || 
                r.State == HexMaster.Attendr.Core.DomainModels.DomainModelState.Modified).ToList();

            if (modifiedRooms.Count > 0)
            {
                await DeleteRoomsAsync(connection, conference.Id, cancellationToken).ConfigureAwait(false);
                foreach (var room in conference.Rooms)
                {
                    await InsertRoomAsync(connection, conference.Id, room, cancellationToken).ConfigureAwait(false);
                }
            }

            // Update speakers: only delete and re-insert if any speaker was created or modified (not just touched)
            var modifiedSpeakers = conference.Speakers.Where(s => 
                s.State == HexMaster.Attendr.Core.DomainModels.DomainModelState.Created || 
                s.State == HexMaster.Attendr.Core.DomainModels.DomainModelState.Modified).ToList();

            if (modifiedSpeakers.Count > 0)
            {
                await DeleteSpeakersAsync(connection, conference.Id, cancellationToken).ConfigureAwait(false);
                foreach (var speaker in conference.Speakers)
                {
                    await InsertSpeakerAsync(connection, conference.Id, speaker, cancellationToken).ConfigureAwait(false);
                }
            }

            // Update presentations: only delete and re-insert if any presentation was created or modified (not just touched)
            var modifiedPresentations = conference.Presentations.Where(p => 
                p.State == HexMaster.Attendr.Core.DomainModels.DomainModelState.Created || 
                p.State == HexMaster.Attendr.Core.DomainModels.DomainModelState.Modified).ToList();

            if (modifiedPresentations.Count > 0)
            {
                await DeletePresentationsAsync(connection, conference.Id, cancellationToken).ConfigureAwait(false);
                foreach (var presentation in conference.Presentations)
                {
                    await InsertPresentationAsync(connection, conference.Id, presentation, cancellationToken).ConfigureAwait(false);

                    foreach (var speakerId in presentation.SpeakerIds)
                    {
                        await InsertPresentationSpeakerAsync(connection, presentation.Id, speakerId, cancellationToken).ConfigureAwait(false);
                    }
                }
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
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Check if conference exists
            var checkSql = "SELECT COUNT(*) FROM conferences WHERE id = @id";
            await using var checkCommand = new NpgsqlCommand(checkSql, connection, transaction);
            checkCommand.Parameters.AddWithValue("@id", id);
            var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)) > 0;

            if (!exists)
            {
                return false;
            }

            // Delete related data (cascade will handle most of this, but being explicit)
            await DeletePresentationsAsync(connection, id, cancellationToken).ConfigureAwait(false);
            await DeleteSpeakersAsync(connection, id, cancellationToken).ConfigureAwait(false);
            await DeleteRoomsAsync(connection, id, cancellationToken).ConfigureAwait(false);

            // Delete the conference itself
            var deleteSql = "DELETE FROM conferences WHERE id = @id";
            await using var deleteCommand = new NpgsqlCommand(deleteSql, connection, transaction);
            deleteCommand.Parameters.AddWithValue("@id", id);
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Guid>> ListActiveConferenceIdsWithSyncSourceAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        const string sql = @"
            SELECT id
            FROM conferences
            WHERE end_date >= @today
              AND sync_source_type IS NOT NULL
              AND sync_source_location_or_api_key IS NOT NULL";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@today", today);

        var conferenceIds = new List<Guid>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            conferenceIds.Add(reader.GetGuid(0));
        }

        return conferenceIds;
    }

    /// <inheritdoc />
    public async Task<(List<Conference> Conferences, int TotalCount)> ListConferencesAsync(
        string? searchQuery,
        int pageNumber,
        int pageSize,
        bool showHidden = false,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Build query with filters
        var whereClause = "WHERE end_date >= @today";
        var parameters = new List<(string Name, object Value)>
        {
            ("@today", today)
        };

        // Filter out hidden conferences unless showHidden is true
        if (!showHidden)
        {
            whereClause += " AND is_visible = true";
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            whereClause += " AND (title ILIKE @search OR city ILIKE @search OR country ILIKE @search)";
            parameters.Add(("@search", $"%{searchQuery}%"));
        }

        // Get total count
        var countSql = $"SELECT COUNT(*) FROM conferences {whereClause}";
        await using var countCommand = new NpgsqlCommand(countSql, connection);
        foreach (var param in parameters)
        {
            countCommand.Parameters.AddWithValue(param.Name, param.Value);
        }
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));

        // Get paginated results
        var sql = $@"
            SELECT id, title, city, country, start_date, end_date, image_url, is_visible,
                   sync_source_type, sync_source_location_or_api_key
            FROM conferences
            {whereClause}
            ORDER BY start_date, title
            LIMIT @pageSize OFFSET @offset";

        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue(param.Name, param.Value);
        }
        command.Parameters.AddWithValue("@pageSize", pageSize);
        command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);

        // Read all conference entities first, then close the reader before loading related data
        var conferenceEntities = new List<ConferenceEntity>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                conferenceEntities.Add(new ConferenceEntity
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    City = reader.GetString(2),
                    Country = reader.GetString(3),
                    StartDate = reader.GetFieldValue<DateOnly>(4),
                    EndDate = reader.GetFieldValue<DateOnly>(5),
                    ImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                    IsVisible = reader.GetBoolean(7),
                    SyncSourceType = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                    SyncSourceLocationOrApiKey = reader.IsDBNull(9) ? null : reader.GetString(9)
                });
            }
        }

        // Now load related entities for each conference
        var conferences = new List<Conference>();
        foreach (var conferenceEntity in conferenceEntities)
        {
            var rooms = await LoadRoomsAsync(connection, conferenceEntity.Id, cancellationToken).ConfigureAwait(false);
            var speakers = await LoadSpeakersAsync(connection, conferenceEntity.Id, cancellationToken).ConfigureAwait(false);
            var presentations = await LoadPresentationsAsync(connection, conferenceEntity.Id, cancellationToken).ConfigureAwait(false);
            var presentationSpeakers = await LoadPresentationSpeakersAsync(connection, conferenceEntity.Id, cancellationToken).ConfigureAwait(false);

            var conference = ConferenceMapper.ToDomain(conferenceEntity, rooms, speakers, presentations, presentationSpeakers);
            conferences.Add(conference);
        }

        return (conferences, totalCount);
    }

    #region Private Helper Methods - Insert Operations

    private static async Task InsertConferenceAsync(NpgsqlConnection connection, ConferenceEntity entity, CancellationToken cancellationToken)
    {
        var sql = @"
            INSERT INTO conferences (id, title, city, country, start_date, end_date, image_url, is_visible, sync_source_type, sync_source_location_or_api_key)
            VALUES (@id, @title, @city, @country, @start_date, @end_date, @image_url, @is_visible, @sync_source_type, @sync_source_location_or_api_key)";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", entity.Id);
        command.Parameters.AddWithValue("@title", entity.Title);
        command.Parameters.AddWithValue("@city", entity.City);
        command.Parameters.AddWithValue("@country", entity.Country);
        command.Parameters.AddWithValue("@start_date", entity.StartDate);
        command.Parameters.AddWithValue("@end_date", entity.EndDate);
        command.Parameters.AddWithValue("@image_url", (object?)entity.ImageUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@is_visible", entity.IsVisible);
        command.Parameters.AddWithValue("@sync_source_type", (object?)entity.SyncSourceType ?? DBNull.Value);
        command.Parameters.AddWithValue("@sync_source_location_or_api_key", (object?)entity.SyncSourceLocationOrApiKey ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task InsertRoomAsync(NpgsqlConnection connection, Guid conferenceId, Room room, CancellationToken cancellationToken)
    {
        var sql = @"
            INSERT INTO rooms (id, conference_id, name, capacity, external_id)
            VALUES (@id, @conference_id, @name, @capacity, @external_id)";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", room.Id);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        command.Parameters.AddWithValue("@name", room.Name);
        command.Parameters.AddWithValue("@capacity", room.Capacity);
        command.Parameters.AddWithValue("@external_id", (object?)room.ExternalId ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task InsertSpeakerAsync(NpgsqlConnection connection, Guid conferenceId, Speaker speaker, CancellationToken cancellationToken)
    {
        var sql = @"
            INSERT INTO speakers (id, conference_id, name, company, profile_picture_url, external_id)
            VALUES (@id, @conference_id, @name, @company, @profile_picture_url, @external_id)";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", speaker.Id);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        command.Parameters.AddWithValue("@name", speaker.Name);
        command.Parameters.AddWithValue("@company", (object?)speaker.Company ?? DBNull.Value);
        command.Parameters.AddWithValue("@profile_picture_url", (object?)speaker.ProfilePictureUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@external_id", (object?)speaker.ExternalId ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task InsertPresentationAsync(NpgsqlConnection connection, Guid conferenceId, Presentation presentation, CancellationToken cancellationToken)
    {
        var sql = @"
            INSERT INTO presentations (id, conference_id, room_id, title, abstract, start_date_time, end_date_time, external_id)
            VALUES (@id, @conference_id, @room_id, @title, @abstract, @start_date_time, @end_date_time, @external_id)";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", presentation.Id);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        command.Parameters.AddWithValue("@room_id", presentation.RoomId);
        command.Parameters.AddWithValue("@title", presentation.Title);
        command.Parameters.AddWithValue("@abstract", presentation.Abstract);
        command.Parameters.AddWithValue("@start_date_time", presentation.StartDateTime);
        command.Parameters.AddWithValue("@end_date_time", presentation.EndDateTime);
        command.Parameters.AddWithValue("@external_id", (object?)presentation.ExternalId ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task InsertPresentationSpeakerAsync(NpgsqlConnection connection, Guid presentationId, Guid speakerId, CancellationToken cancellationToken)
    {
        var sql = @"
            INSERT INTO presentation_speakers (presentation_id, speaker_id)
            VALUES (@presentation_id, @speaker_id)";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@presentation_id", presentationId);
        command.Parameters.AddWithValue("@speaker_id", speakerId);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Private Helper Methods - Load Operations

    private static async Task<ConferenceEntity?> LoadConferenceAsync(NpgsqlConnection connection, Guid id, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT id, title, city, country, start_date, end_date, image_url, is_visible,
                   sync_source_type, sync_source_location_or_api_key
            FROM conferences
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return new ConferenceEntity
        {
            Id = reader.GetGuid(0),
            Title = reader.GetString(1),
            City = reader.GetString(2),
            Country = reader.GetString(3),
            StartDate = reader.GetFieldValue<DateOnly>(4),
            EndDate = reader.GetFieldValue<DateOnly>(5),
            ImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
            IsVisible = reader.GetBoolean(7),
            SyncSourceType = reader.IsDBNull(8) ? null : reader.GetInt32(8),
            SyncSourceLocationOrApiKey = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    private static async Task<List<RoomEntity>> LoadRoomsAsync(NpgsqlConnection connection, Guid conferenceId, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT id, conference_id, name, capacity, external_id
            FROM rooms
            WHERE conference_id = @conference_id
            ORDER BY name";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);

        var rooms = new List<RoomEntity>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            rooms.Add(new RoomEntity
            {
                Id = reader.GetGuid(0),
                ConferenceId = reader.GetGuid(1),
                Name = reader.GetString(2),
                Capacity = reader.GetInt32(3),
                ExternalId = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        return rooms;
    }

    private static async Task<List<SpeakerEntity>> LoadSpeakersAsync(NpgsqlConnection connection, Guid conferenceId, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT id, conference_id, name, company, profile_picture_url, external_id
            FROM speakers
            WHERE conference_id = @conference_id
            ORDER BY name";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);

        var speakers = new List<SpeakerEntity>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            speakers.Add(new SpeakerEntity
            {
                Id = reader.GetGuid(0),
                ConferenceId = reader.GetGuid(1),
                Name = reader.GetString(2),
                Company = reader.IsDBNull(3) ? null : reader.GetString(3),
                ProfilePictureUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                ExternalId = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return speakers;
    }

    private static async Task<List<PresentationEntity>> LoadPresentationsAsync(NpgsqlConnection connection, Guid conferenceId, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT id, conference_id, room_id, title, abstract, start_date_time, end_date_time, external_id
            FROM presentations
            WHERE conference_id = @conference_id
            ORDER BY start_date_time, title";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);

        var presentations = new List<PresentationEntity>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            presentations.Add(new PresentationEntity
            {
                Id = reader.GetGuid(0),
                ConferenceId = reader.GetGuid(1),
                RoomId = reader.GetGuid(2),
                Title = reader.GetString(3),
                Abstract = reader.GetString(4),
                StartDateTime = reader.GetDateTime(5),
                EndDateTime = reader.GetDateTime(6),
                ExternalId = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }

        return presentations;
    }

    private static async Task<Dictionary<Guid, List<Guid>>> LoadPresentationSpeakersAsync(NpgsqlConnection connection, Guid conferenceId, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT ps.presentation_id, ps.speaker_id
            FROM presentation_speakers ps
            INNER JOIN presentations p ON ps.presentation_id = p.id
            WHERE p.conference_id = @conference_id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);

        var result = new Dictionary<Guid, List<Guid>>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var presentationId = reader.GetGuid(0);
            var speakerId = reader.GetGuid(1);

            if (!result.ContainsKey(presentationId))
            {
                result[presentationId] = new List<Guid>();
            }

            result[presentationId].Add(speakerId);
        }

        return result;
    }

    #endregion

    #region Private Helper Methods - Update Operations

    private static async Task UpdateConferenceAsync(NpgsqlConnection connection, ConferenceEntity entity, CancellationToken cancellationToken)
    {
        var sql = @"
            UPDATE conferences
            SET title = @title, city = @city, country = @country, 
                start_date = @start_date, end_date = @end_date, image_url = @image_url, is_visible = @is_visible,
                sync_source_type = @sync_source_type, sync_source_location_or_api_key = @sync_source_location_or_api_key
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", entity.Id);
        command.Parameters.AddWithValue("@title", entity.Title);
        command.Parameters.AddWithValue("@city", entity.City);
        command.Parameters.AddWithValue("@country", entity.Country);
        command.Parameters.AddWithValue("@start_date", entity.StartDate);
        command.Parameters.AddWithValue("@end_date", entity.EndDate);
        command.Parameters.AddWithValue("@image_url", (object?)entity.ImageUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@is_visible", entity.IsVisible);
        command.Parameters.AddWithValue("@sync_source_type", (object?)entity.SyncSourceType ?? DBNull.Value);
        command.Parameters.AddWithValue("@sync_source_location_or_api_key", (object?)entity.SyncSourceLocationOrApiKey ?? DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Conference with ID {entity.Id} does not exist.");
        }
    }

    #endregion

    #region Private Helper Methods - Delete Operations

    private static async Task DeleteRoomsAsync(NpgsqlConnection connection, Guid conferenceId, CancellationToken cancellationToken)
    {
        var sql = "DELETE FROM rooms WHERE conference_id = @conference_id";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task DeleteSpeakersAsync(NpgsqlConnection connection, Guid conferenceId, CancellationToken cancellationToken)
    {
        var sql = "DELETE FROM speakers WHERE conference_id = @conference_id";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task DeletePresentationsAsync(NpgsqlConnection connection, Guid conferenceId, CancellationToken cancellationToken)
    {
        // Delete presentation_speakers first (FK constraint)
        var sqlJunction = @"
            DELETE FROM presentation_speakers 
            WHERE presentation_id IN (
                SELECT id FROM presentations WHERE conference_id = @conference_id
            )";
        await using var junctionCommand = new NpgsqlCommand(sqlJunction, connection);
        junctionCommand.Parameters.AddWithValue("@conference_id", conferenceId);
        await junctionCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        // Delete presentations
        var sql = "DELETE FROM presentations WHERE conference_id = @conference_id";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Database Initialization

    private async Task EnsureTablesExistAsync()
    {
        await using var connection = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);

        // Create conferences table
        var createConferencesSql = @"
            CREATE TABLE IF NOT EXISTS conferences (
                id UUID PRIMARY KEY,
                title VARCHAR(500) NOT NULL,
                city VARCHAR(200) NOT NULL,
                country VARCHAR(200) NOT NULL,
                start_date DATE NOT NULL,
                end_date DATE NOT NULL,
                image_url VARCHAR(1000),
                sync_source_type INTEGER,
                sync_source_location_or_api_key VARCHAR(1000)
            )";
        await using (var command = new NpgsqlCommand(createConferencesSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // Create rooms table
        var createRoomsSql = @"
            CREATE TABLE IF NOT EXISTS rooms (
                id UUID PRIMARY KEY,
                conference_id UUID NOT NULL,
                name VARCHAR(200) NOT NULL,
                capacity INTEGER NOT NULL,
                external_id VARCHAR(200),
                FOREIGN KEY (conference_id) REFERENCES conferences(id) ON DELETE CASCADE
            )";
        await using (var command = new NpgsqlCommand(createRoomsSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // Create speakers table
        var createSpeakersSql = @"
            CREATE TABLE IF NOT EXISTS speakers (
                id UUID PRIMARY KEY,
                conference_id UUID NOT NULL,
                name VARCHAR(500) NOT NULL,
                company VARCHAR(500),
                profile_picture_url VARCHAR(1000),
                external_id VARCHAR(200),
                FOREIGN KEY (conference_id) REFERENCES conferences(id) ON DELETE CASCADE
            )";
        await using (var command = new NpgsqlCommand(createSpeakersSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // Create presentations table
        var createPresentationsSql = @"
            CREATE TABLE IF NOT EXISTS presentations (
                id UUID PRIMARY KEY,
                conference_id UUID NOT NULL,
                room_id UUID NOT NULL,
                title VARCHAR(500) NOT NULL,
                abstract TEXT NOT NULL,
                start_date_time TIMESTAMP NOT NULL,
                end_date_time TIMESTAMP NOT NULL,
                external_id VARCHAR(200),
                FOREIGN KEY (conference_id) REFERENCES conferences(id) ON DELETE CASCADE,
                FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE CASCADE
            )";
        await using (var command = new NpgsqlCommand(createPresentationsSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // Create presentation_speakers junction table
        var createPresentationSpeakersSql = @"
            CREATE TABLE IF NOT EXISTS presentation_speakers (
                presentation_id UUID NOT NULL,
                speaker_id UUID NOT NULL,
                PRIMARY KEY (presentation_id, speaker_id),
                FOREIGN KEY (presentation_id) REFERENCES presentations(id) ON DELETE CASCADE,
                FOREIGN KEY (speaker_id) REFERENCES speakers(id) ON DELETE CASCADE
            )";
        await using (var command = new NpgsqlCommand(createPresentationSpeakersSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // Create indexes for better query performance
        var createIndexesSql = @"
            CREATE INDEX IF NOT EXISTS idx_conferences_end_date ON conferences(end_date);
            CREATE INDEX IF NOT EXISTS idx_conferences_start_date ON conferences(start_date);
            CREATE INDEX IF NOT EXISTS idx_rooms_conference_id ON rooms(conference_id);
            CREATE INDEX IF NOT EXISTS idx_speakers_conference_id ON speakers(conference_id);
            CREATE INDEX IF NOT EXISTS idx_presentations_conference_id ON presentations(conference_id);
            CREATE INDEX IF NOT EXISTS idx_presentations_start_date_time ON presentations(start_date_time);
            CREATE INDEX IF NOT EXISTS idx_presentation_speakers_speaker_id ON presentation_speakers(speaker_id);
        ";
        await using (var command = new NpgsqlCommand(createIndexesSql, connection))
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }

    #endregion
}
