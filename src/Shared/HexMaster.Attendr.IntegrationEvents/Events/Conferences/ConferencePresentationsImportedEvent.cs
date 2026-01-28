using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events.Conferences;

/// <summary>
/// Integration event published after all presentations have been successfully imported for a conference.
/// This signals that the conference synchronization is complete and profile topic matching can begin.
/// </summary>
public sealed class ConferencePresentationsImportedEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.ConferencePresentationsImported;

    /// <summary>
    /// Gets the ID of the conference for which presentations were imported.
    /// </summary>
    public Guid ConferenceId { get; init; }

    /// <summary>
    /// Gets the name of the conference.
    /// </summary>
    public string ConferenceName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the collection of profile IDs that are following this conference.
    /// These profiles should have their topic recommendations updated.
    /// </summary>
    public IReadOnlyCollection<Guid> ProfileIds { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets the number of presentations that were imported for this conference.
    /// </summary>
    public int PresentationsCount { get; init; }
}
