using HexMaster.Attendr.Conferences.Data.Postgres.Entities;
using HexMaster.Attendr.Conferences.DomainModels;
using Npgsql;

namespace HexMaster.Attendr.Conferences.Data.Postgres;

/// <summary>
/// PostgreSQL implementation of ITopicsRepository.
/// </summary>
public sealed class PostgresTopicsRepository : ITopicsRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresTopicsRepository(NpgsqlDataSource dataSource)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
    }

    public async Task<Topic> GetOrCreateTopicAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // Try to get existing topic by exact name match
        var selectSql = "SELECT id, key, name, is_visible, created_on FROM topics WHERE name = @name";
        await using var selectCommand = new NpgsqlCommand(selectSql, connection);
        selectCommand.Parameters.AddWithValue("@name", name);

        await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return Topic.FromPersisted(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetBoolean(3),
                reader.GetFieldValue<DateTimeOffset>(4));
        }
        await reader.CloseAsync();

        // Create new topic if not found (AI-created topics are hidden by default)
        // Generate a normalized key from the name: lowercase, replace spaces/special chars with hyphens
        var normalizedKey = NormalizeKey(name);
        var topic = Topic.Create(normalizedKey, name);
        return await PersistNewTopicAsync(connection, topic, cancellationToken);
    }

    private static string NormalizeKey(string name)
    {
        // Lowercase, replace whitespace and non-alphanumeric characters with hyphens, collapse multiple hyphens
        var normalized = System.Text.RegularExpressions.Regex.Replace(name.ToLowerInvariant(), @"[^a-z0-9]+", "-");
        return normalized.Trim('-');
    }

    public async Task<Topic> CreateTopicAsync(Topic topic, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        return await PersistNewTopicAsync(connection, topic, cancellationToken);
    }

    private static async Task<Topic> PersistNewTopicAsync(NpgsqlConnection connection, Topic topic, CancellationToken cancellationToken)
    {
        var insertSql = @"
            INSERT INTO topics (id, key, name, is_visible, created_on)
            VALUES (@id, @key, @name, @is_visible, @created_on)";

        await using var insertCommand = new NpgsqlCommand(insertSql, connection);
        insertCommand.Parameters.AddWithValue("@id", topic.Id);
        insertCommand.Parameters.AddWithValue("@key", topic.Key);
        insertCommand.Parameters.AddWithValue("@name", topic.Name);
        insertCommand.Parameters.AddWithValue("@is_visible", topic.IsVisible);
        insertCommand.Parameters.AddWithValue("@created_on", topic.CreatedOn);

        await insertCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        return topic;
    }

    public async Task LinkPresentationToTopicAsync(Guid presentationId, Guid topicId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"
            INSERT INTO presentation_topics (id, presentation_id, topic_id)
            VALUES (@id, @presentation_id, @topic_id)
            ON CONFLICT (presentation_id, topic_id) DO NOTHING";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", Guid.NewGuid());
        command.Parameters.AddWithValue("@presentation_id", presentationId);
        command.Parameters.AddWithValue("@topic_id", topicId);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<(Guid ConferenceId, Presentation Presentation)>> GetUnanalysedPresentationsAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"
            SELECT p.id, p.conference_id, p.room_id, p.title, p.abstract, 
                   p.start_date_time, p.end_date_time, p.is_analysed, p.external_id
            FROM presentations p
            WHERE p.is_analysed = false
            ORDER BY p.start_date_time DESC
            LIMIT @batch_size";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@batch_size", batchSize);

        var presentationData = new List<(Guid PresentationId, Guid ConferenceId, Guid RoomId, string Title, string Abstract, DateTime StartDateTime, DateTime EndDateTime, string? ExternalId, bool IsAnalysed)>();

        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                presentationData.Add((
                    reader.GetGuid(0),  // presentation_id
                    reader.GetGuid(1),  // conference_id
                    reader.GetGuid(2),  // room_id
                    reader.GetString(3),  // title
                    reader.GetString(4),  // abstract
                    reader.GetDateTime(5),  // start_date_time
                    reader.GetDateTime(6),  // end_date_time
                    reader.IsDBNull(8) ? null : reader.GetString(8),  // external_id
                    reader.GetBoolean(7)  // is_analysed
                ));
            }
        }

        var results = new List<(Guid ConferenceId, Presentation Presentation)>();

        foreach (var data in presentationData)
        {
            var speakers = await LoadPresentationSpeakersAsync(connection, data.PresentationId, cancellationToken).ConfigureAwait(false);
            var topics = await LoadPresentationTopicsAsync(connection, data.PresentationId, cancellationToken).ConfigureAwait(false);
            var roomEntity = await LoadRoomAsync(connection, data.RoomId, cancellationToken).ConfigureAwait(false);
            var room = Room.FromPersisted(roomEntity.Id, roomEntity.Name, roomEntity.Capacity, roomEntity.ExternalId);

            var presentation = Presentation.FromPersisted(
                data.PresentationId,
                data.Title,
                data.Abstract,
                data.StartDateTime,
                data.EndDateTime,
                room,
                speakers,
                data.ExternalId,
                topics.Select(t => new PresentationTopic(t.Key, t.Name)).ToList(),
                data.IsAnalysed);

            results.Add((data.ConferenceId, presentation));
        }

        return results;
    }

    public async Task MarkPresentationAsAnalysedAsync(Guid presentationId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = "UPDATE presentations SET is_analysed = true WHERE id = @id";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", presentationId);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<List<Speaker>> LoadPresentationSpeakersAsync(NpgsqlConnection connection, Guid presentationId, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT s.id, s.name, s.company, s.profile_picture_url, s.external_id
            FROM presentation_speakers ps
            INNER JOIN speakers s ON ps.speaker_id = s.id
            WHERE ps.presentation_id = @presentation_id";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@presentation_id", presentationId);

        var speakers = new List<Speaker>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var speaker = Speaker.FromPersisted(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4));
            speakers.Add(speaker);
        }

        return speakers;
    }

    public async Task<Topic?> GetTopicByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = "SELECT id, key, name, is_visible, created_on FROM topics WHERE id = @id";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return Topic.FromPersisted(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetBoolean(3),
                reader.GetFieldValue<DateTimeOffset>(4));
        }

        return null;
    }

    public async Task<List<Topic>> ListTopicsAsync(bool onlyVisible = true, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"
            SELECT id, key, name, is_visible, created_on FROM topics
            WHERE is_visible = @is_visible OR @is_visible = false
            ORDER BY created_on DESC";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@is_visible", onlyVisible);

        var topics = new List<Topic>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            topics.Add(Topic.FromPersisted(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetBoolean(3),
                reader.GetFieldValue<DateTimeOffset>(4)));
        }

        return topics;
    }

    public async Task UpdateTopicAsync(Topic topic, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(topic);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"
            UPDATE topics 
            SET key = @key, name = @name, is_visible = @is_visible 
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", topic.Id);
        command.Parameters.AddWithValue("@key", topic.Key);
        command.Parameters.AddWithValue("@name", topic.Name);
        command.Parameters.AddWithValue("@is_visible", topic.IsVisible);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DeleteTopicAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = "DELETE FROM topics WHERE id = @id";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rowsAffected > 0;
    }

    public async Task DeleteTopicPresentationReferencesAsync(Guid topicId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sql = "DELETE FROM presentation_topics WHERE topic_id = @topic_id";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@topic_id", topicId);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<(Guid ConferenceId, Guid PresentationId)>> GetFuturePresentationsByTopicIdAsync(Guid topicId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // Get all future presentations that have this topic
        var sql = @"
            SELECT DISTINCT p.id, p.conference_id
            FROM presentations p
            INNER JOIN presentation_topics pt ON p.id = pt.presentation_id
            WHERE pt.topic_id = @topic_id
              AND p.start_date_time > @now
            ORDER BY p.id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@topic_id", topicId);
        command.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow);

        var results = new List<(Guid ConferenceId, Guid PresentationId)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add((reader.GetGuid(1), reader.GetGuid(0))); // ConferenceId, PresentationId
        }

        return results;
    }

    private static async Task<List<(string Key, string Name)>> LoadPresentationTopicsAsync(NpgsqlConnection connection, Guid presentationId, CancellationToken cancellationToken)
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

    private static async Task<RoomEntity> LoadRoomAsync(NpgsqlConnection connection, Guid roomId, CancellationToken cancellationToken)
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
}
