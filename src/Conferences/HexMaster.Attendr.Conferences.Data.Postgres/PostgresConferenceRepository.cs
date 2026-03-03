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

                foreach (var speaker in presentation.Speakers)
                {
                    await InsertPresentationSpeakerAsync(connection, presentation.Id, speaker.Id, cancellationToken).ConfigureAwait(false);
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
        var presentationTopics = await LoadPresentationTopicsAsync(connection, id, cancellationToken).ConfigureAwait(false);

        return ConferenceMapper.ToDomain(conferenceEntity, rooms, speakers, presentations, presentationSpeakers, presentationTopics);
    }

    /// <inheritdoc />
    public async Task<Presentation?> GetPresentationByIdAsync(Guid conferenceId, Guid presentationId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // Load presentation entity
        var presentationEntity = await LoadPresentationByIdAsync(connection, conferenceId, presentationId, cancellationToken).ConfigureAwait(false);
        if (presentationEntity == null)
        {
            return null;
        }

        // Load speakers for the conference (needed to construct Speaker objects)
        var speakers = await LoadSpeakersAsync(connection, conferenceId, cancellationToken).ConfigureAwait(false);

        // Load presentation speakers
        var presentationSpeakerIds = await LoadPresentationSpeakerIdsForSinglePresentationAsync(connection, presentationId, cancellationToken).ConfigureAwait(false);

        // Get speaker objects for this presentation
        var presentationSpeakers = speakers
            .Where(s => presentationSpeakerIds.Contains(s.Id))
            .Select(s => Speaker.FromPersisted(s.Id, s.Name, s.Company, s.ProfilePictureUrl, s.ExternalId))
            .ToList();

        // Load topics
        var topics = await LoadPresentationTopicsForSinglePresentationAsync(connection, presentationId, cancellationToken).ConfigureAwait(false);

        var roomEntity = await LoadRoomForPresentationAsync(connection, presentationEntity.RoomId, cancellationToken).ConfigureAwait(false);
        var room = Room.FromPersisted(roomEntity.Id, roomEntity.Name, roomEntity.Capacity, roomEntity.ExternalId);

        return Presentation.FromPersisted(
            presentationEntity.Id,
            presentationEntity.Title,
            presentationEntity.Abstract,
            presentationEntity.StartDateTime,
            presentationEntity.EndDateTime,
            room,
            presentationSpeakers,
            presentationEntity.ExternalId,
            topics.Select(t => new PresentationTopic(t.Key, t.Name)).ToList(),
            presentationEntity.IsAnalysed);
    }

    /// <inheritdoc />
    public async Task<ConferenceDetailsDto?> GetDetailsByIdAsync(Guid id, Guid? currentProfileId = null, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // Load conference (no visibility filter here; we apply it below)
        var conferenceEntity = await LoadConferenceAsync(connection, id, cancellationToken).ConfigureAwait(false);
        if (conferenceEntity == null)
        {
            return null;
        }

        // Apply visibility rules:
        // - Visible conferences are always returned
        // - Unpublished conferences are returned when the caller is an admin OR the owner
        if (!conferenceEntity.IsVisible)
        {
            var isOwner = currentProfileId.HasValue
                && conferenceEntity.CreatedByProfileId.HasValue
                && conferenceEntity.CreatedByProfileId.Value == currentProfileId.Value;

            if (!isAdmin && !isOwner)
            {
                return null;
            }
        }

        // Load related entities
        var rooms = await LoadRoomsAsync(connection, id, cancellationToken).ConfigureAwait(false);
        var speakers = await LoadSpeakersAsync(connection, id, cancellationToken).ConfigureAwait(false);
        var presentations = await LoadPresentationsAsync(connection, id, cancellationToken).ConfigureAwait(false);
        var presentationSpeakers = await LoadPresentationSpeakersAsync(connection, id, cancellationToken).ConfigureAwait(false);
        var presentationTopics = await LoadPresentationTopicsAsync(connection, id, cancellationToken).ConfigureAwait(false);

        return ConferenceDtoMapper.ToDetailsDto(conferenceEntity, rooms, speakers, presentations, presentationSpeakers, presentationTopics);
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

            // Update speakers based on their individual state
            foreach (var speaker in conference.Speakers)
            {
                if (speaker.State == HexMaster.Attendr.Core.DomainModels.DomainModelState.Created)
                {
                    await InsertSpeakerAsync(connection, conference.Id, speaker, cancellationToken).ConfigureAwait(false);
                }
                else if (speaker.State == HexMaster.Attendr.Core.DomainModels.DomainModelState.Modified)
                {
                    await UpdateSpeakerAsync(connection, speaker, cancellationToken).ConfigureAwait(false);
                }
                else if (speaker.State == HexMaster.Attendr.Core.DomainModels.DomainModelState.Deleted)
                {
                    await DeleteSpeakerByIdAsync(connection, speaker.Id, cancellationToken).ConfigureAwait(false);
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

                    foreach (var speaker in presentation.Speakers)
                    {
                        await InsertPresentationSpeakerAsync(connection, presentation.Id, speaker.Id, cancellationToken).ConfigureAwait(false);
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
        Guid? currentProfileId = null,
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

        // Filter: visible OR owned by current user (when not showing all hidden)
        if (!showHidden)
        {
            if (currentProfileId.HasValue && currentProfileId.Value != Guid.Empty)
            {
                whereClause += " AND (is_visible = true OR (created_by_profile_id = @currentProfileId AND created_by_profile_id IS NOT NULL))";
                parameters.Add(("@currentProfileId", currentProfileId.Value));
            }
            else
            {
                whereClause += " AND is_visible = true";
            }
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
                   sync_source_type, sync_source_location_or_api_key, created_by_profile_id
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
                    SyncSourceLocationOrApiKey = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CreatedByProfileId = reader.IsDBNull(10) ? null : reader.GetGuid(10)
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
            var presentationTopics = await LoadPresentationTopicsAsync(connection, conferenceEntity.Id, cancellationToken).ConfigureAwait(false);

            var conference = ConferenceMapper.ToDomain(conferenceEntity, rooms, speakers, presentations, presentationSpeakers, presentationTopics);
            conferences.Add(conference);
        }

        return (conferences, totalCount);
    }

    /// <inheritdoc />
    public async Task<ConferenceMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        const string sql = @"
            SELECT
                (SELECT COUNT(*) FROM conferences WHERE is_visible = true) AS visible_conferences,
                (SELECT COUNT(*) FROM conferences WHERE is_visible = true AND start_date > @today) AS upcoming_conferences,
                (SELECT COUNT(*) FROM presentations p INNER JOIN conferences c ON p.conference_id = c.id WHERE c.is_visible = true) AS total_presentations,
                (SELECT COUNT(DISTINCT s.id) FROM speakers s INNER JOIN conferences c ON s.conference_id = c.id WHERE c.is_visible = true) AS total_speakers,
                (SELECT COUNT(*) FROM rooms r INNER JOIN conferences c ON r.conference_id = c.id WHERE c.is_visible = true) AS total_rooms,
                (SELECT COUNT(*) FROM topics WHERE is_visible = true) AS total_topics";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@today", today);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);

        return new ConferenceMetricsDto(
            VisibleConferences: Convert.ToInt32(reader.GetInt64(0)),
            UpcomingConferences: Convert.ToInt32(reader.GetInt64(1)),
            TotalPresentations: Convert.ToInt32(reader.GetInt64(2)),
            TotalSpeakers: Convert.ToInt32(reader.GetInt64(3)),
            TotalRooms: Convert.ToInt32(reader.GetInt64(4)),
            TotalTopics: Convert.ToInt32(reader.GetInt64(5)));
    }

    #region Private Helper Methods - Insert Operations

    private static async Task InsertConferenceAsync(NpgsqlConnection connection, ConferenceEntity entity, CancellationToken cancellationToken)
    {
        var sql = @"
            INSERT INTO conferences (id, title, city, country, start_date, end_date, image_url, is_visible, sync_source_type, sync_source_location_or_api_key, created_by_profile_id)
            VALUES (@id, @title, @city, @country, @start_date, @end_date, @image_url, @is_visible, @sync_source_type, @sync_source_location_or_api_key, @created_by_profile_id)";

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
        command.Parameters.AddWithValue("@created_by_profile_id", (object?)entity.CreatedByProfileId ?? DBNull.Value);

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

    private static async Task UpdateSpeakerAsync(NpgsqlConnection connection, Speaker speaker, CancellationToken cancellationToken)
    {
        var sql = @"
            UPDATE speakers
            SET name = @name,
                company = @company,
                profile_picture_url = @profile_picture_url,
                external_id = @external_id
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", speaker.Id);
        command.Parameters.AddWithValue("@name", speaker.Name);
        command.Parameters.AddWithValue("@company", (object?)speaker.Company ?? DBNull.Value);
        command.Parameters.AddWithValue("@profile_picture_url", (object?)speaker.ProfilePictureUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@external_id", (object?)speaker.ExternalId ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task DeleteSpeakerByIdAsync(NpgsqlConnection connection, Guid speakerId, CancellationToken cancellationToken)
    {
        var sql = "DELETE FROM speakers WHERE id = @id";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", speakerId);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task InsertPresentationAsync(NpgsqlConnection connection, Guid conferenceId, Presentation presentation, CancellationToken cancellationToken)
    {
        var sql = @"
            INSERT INTO presentations (id, conference_id, room_id, title, abstract, start_date_time, end_date_time, is_analysed, external_id)
            VALUES (@id, @conference_id, @room_id, @title, @abstract, @start_date_time, @end_date_time, @is_analysed, @external_id)";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", presentation.Id);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        command.Parameters.AddWithValue("@room_id", presentation.Room.Id);
        command.Parameters.AddWithValue("@title", presentation.Title);
        command.Parameters.AddWithValue("@abstract", presentation.Abstract);
        command.Parameters.AddWithValue("@start_date_time", presentation.StartDateTime);
        command.Parameters.AddWithValue("@end_date_time", presentation.EndDateTime);
        command.Parameters.AddWithValue("@is_analysed", false);
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
                   sync_source_type, sync_source_location_or_api_key, created_by_profile_id
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
            SyncSourceLocationOrApiKey = reader.IsDBNull(9) ? null : reader.GetString(9),
            CreatedByProfileId = reader.IsDBNull(10) ? null : reader.GetGuid(10)
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

    private static async Task<RoomEntity> LoadRoomForPresentationAsync(NpgsqlConnection connection, Guid roomId, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT id, conference_id, name, capacity, external_id
            FROM rooms
            WHERE id = @room_id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@room_id", roomId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return new RoomEntity
            {
                Id = reader.GetGuid(0),
                ConferenceId = reader.GetGuid(1),
                Name = reader.GetString(2),
                Capacity = reader.GetInt32(3),
                ExternalId = reader.IsDBNull(4) ? null : reader.GetString(4)
            };
        }

        throw new InvalidOperationException($"Room with ID {roomId} not found");
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
            SELECT id, conference_id, room_id, title, abstract, start_date_time, end_date_time, is_analysed, external_id
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
                IsAnalysed = reader.GetBoolean(7),
                ExternalId = reader.IsDBNull(8) ? null : reader.GetString(8)
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

    private static async Task<Dictionary<Guid, List<(string Key, string Name)>>> LoadPresentationTopicsAsync(NpgsqlConnection connection, Guid conferenceId, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT pt.presentation_id, t.key, t.name
            FROM presentation_topics pt
            INNER JOIN presentations p ON pt.presentation_id = p.id
            INNER JOIN topics t ON pt.topic_id = t.id
            WHERE p.conference_id = @conference_id
              AND t.is_visible = true
            ORDER BY pt.presentation_id, t.key";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);

        var result = new Dictionary<Guid, List<(string Key, string Name)>>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var presentationId = reader.GetGuid(0);
            var key = reader.GetString(1);
            var name = reader.GetString(2);

            if (!result.ContainsKey(presentationId))
            {
                result[presentationId] = new List<(string, string)>();
            }

            result[presentationId].Add((key, name));
        }

        return result;
    }

    private static async Task<PresentationEntity?> LoadPresentationByIdAsync(NpgsqlConnection connection, Guid conferenceId, Guid presentationId, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT id, conference_id, room_id, title, abstract, start_date_time, end_date_time, is_analysed, external_id
            FROM presentations
            WHERE conference_id = @conference_id AND id = @presentation_id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@conference_id", conferenceId);
        command.Parameters.AddWithValue("@presentation_id", presentationId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return new PresentationEntity
            {
                Id = reader.GetGuid(0),
                ConferenceId = reader.GetGuid(1),
                RoomId = reader.GetGuid(2),
                Title = reader.GetString(3),
                Abstract = reader.GetString(4),
                StartDateTime = reader.GetDateTime(5),
                EndDateTime = reader.GetDateTime(6),
                IsAnalysed = reader.GetBoolean(7),
                ExternalId = reader.IsDBNull(8) ? null : reader.GetString(8)
            };
        }

        return null;
    }

    private static async Task<List<Guid>> LoadPresentationSpeakerIdsForSinglePresentationAsync(NpgsqlConnection connection, Guid presentationId, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT speaker_id
            FROM presentation_speakers
            WHERE presentation_id = @presentation_id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@presentation_id", presentationId);

        var speakerIds = new List<Guid>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            speakerIds.Add(reader.GetGuid(0));
        }

        return speakerIds;
    }

    private static async Task<List<(string Key, string Name)>> LoadPresentationTopicsForSinglePresentationAsync(NpgsqlConnection connection, Guid presentationId, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT t.key, t.name
            FROM presentation_topics pt
            INNER JOIN topics t ON pt.topic_id = t.id
            WHERE pt.presentation_id = @presentation_id
              AND t.is_visible = true
            ORDER BY t.key";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@presentation_id", presentationId);

        var topics = new List<(string Key, string Name)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            topics.Add((reader.GetString(0), reader.GetString(1)));
        }

        return topics;
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
}
