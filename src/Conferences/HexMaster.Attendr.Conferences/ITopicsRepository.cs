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
}
