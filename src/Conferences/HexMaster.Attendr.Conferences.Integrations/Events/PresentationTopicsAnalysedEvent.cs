using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.Conferences.Integrations.Events;

/// <summary>
/// Integration event published when a presentation's topics have been analysed.
/// </summary>
public sealed class PresentationTopicsAnalysedEvent : IntegrationEvent
{
    public override string EventType => "Conferences.PresentationTopicsAnalysed";

    /// <summary>
    /// Gets the conference ID.
    /// </summary>
    public required Guid ConferenceId { get; init; }

    /// <summary>
    /// Gets the presentation ID.
    /// </summary>
    public required Guid PresentationId { get; init; }

    /// <summary>
    /// Gets the presentation title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the presentation abstract.
    /// </summary>
    public required string Abstract { get; init; }

    /// <summary>
    /// Gets the list of identified topics.
    /// </summary>
    public required List<string> Topics { get; init; }
}
