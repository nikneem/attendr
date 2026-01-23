using HexMaster.Attendr.Conferences.DomainModels;

namespace HexMaster.Attendr.Conferences;

/// <summary>
/// Repository interface for topic operations.
/// </summary>
public interface ITopicsRepository
{
    /// <summary>
    /// Gets or creates a topic by its key.
    /// </summary>
    /// <param name="key">The unique key of the topic.</param>
    /// <param name="name">The display name of the topic.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The existing or newly created topic.</returns>
    Task<Topic> GetOrCreateTopicAsync(string key, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Links a presentation to a topic.
    /// </summary>
    /// <param name="presentationId">The presentation ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task LinkPresentationToTopicAsync(Guid presentationId, Guid topicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unanalysed presentations.
    /// </summary>
    /// <param name="batchSize">Maximum number of presentations to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of unanalysed presentations with their conference IDs.</returns>
    Task<List<(Guid ConferenceId, Presentation Presentation)>> GetUnanalysedPresentationsAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a presentation as analysed.
    /// </summary>
    /// <param name="presentationId">The presentation ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task MarkPresentationAsAnalysedAsync(Guid presentationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a topic by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the topic.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The topic if found; otherwise null.</returns>
    Task<Topic?> GetTopicByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all topics.
    /// </summary>
    /// <param name="onlyVisible">If true, only return visible topics.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of topics.</returns>
    Task<List<Topic>> ListTopicsAsync(bool onlyVisible = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing topic.
    /// </summary>
    /// <param name="topic">The topic to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateTopicAsync(Topic topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a topic by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the topic to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the topic was deleted; otherwise false.</returns>
    Task<bool> DeleteTopicAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all presentation-topic references for a given topic.
    /// Used for cascade delete when a topic is deleted.
    /// </summary>
    /// <param name="topicId">The unique identifier of the topic.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteTopicPresentationReferencesAsync(Guid topicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all presentations linked to a topic that are scheduled in the future.
    /// </summary>
    /// <param name="topicId">The unique identifier of the topic.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of tuples containing conference IDs and presentation IDs.</returns>
    Task<List<(Guid ConferenceId, Guid PresentationId)>> GetFuturePresentationsByTopicIdAsync(Guid topicId, CancellationToken cancellationToken = default);
}
