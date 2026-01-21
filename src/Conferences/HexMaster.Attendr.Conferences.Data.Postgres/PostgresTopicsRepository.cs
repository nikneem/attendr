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

    public async Task<Topic> GetOrCreateTopicAsync(string key, string name, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // Try to get existing topic
        var selectSql = "SELECT id, key, name, is_visible, created_on FROM topics WHERE key = @key";
        await using var selectCommand = new NpgsqlCommand(selectSql, connection);
        selectCommand.Parameters.AddWithValue("@key", key);

        await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return Topic.FromPersisted(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetBoolean(3),
                reader.GetDateTime(4));
        }
        await reader.CloseAsync();

        // Create new topic if not found
        var topic = Topic.Create(key, name);

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
            var speakerIds = await LoadPresentationSpeakerIdsAsync(connection, data.PresentationId, cancellationToken).ConfigureAwait(false);

            var presentation = Presentation.FromPersisted(
                data.PresentationId,
                data.Title,
                data.Abstract,
                data.StartDateTime,
                data.EndDateTime,
                data.RoomId,
                speakerIds,
                data.ExternalId,
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

    private static async Task<List<Guid>> LoadPresentationSpeakerIdsAsync(NpgsqlConnection connection, Guid presentationId, CancellationToken cancellationToken)
    {
        var sql = "SELECT speaker_id FROM presentation_speakers WHERE presentation_id = @presentation_id";
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
                reader.GetDateTime(4));
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
                reader.GetDateTime(4)));
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
}
