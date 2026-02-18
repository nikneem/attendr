using HexMaster.Attendr.IntegrationEvents.Constants;
using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.IntegrationEvents.Events.Topics;

/// <summary>
/// Integration event raised when a topic is created or updated.
/// This event is published whenever a topic's details change, including on initial creation.
/// </summary>
public sealed class TopicChangedEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.TopicChanged;

    /// <summary>
    /// Gets the unique identifier of the topic.
    /// </summary>
    public Guid TopicId { get; init; }

    /// <summary>
    /// Gets the unique key of the topic used for lookups and references.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of the topic.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the topic is publicly visible.
    /// </summary>
    public bool IsVisible { get; init; }
}
