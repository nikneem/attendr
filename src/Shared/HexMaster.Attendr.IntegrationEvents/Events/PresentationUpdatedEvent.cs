namespace HexMaster.Attendr.IntegrationEvents.Events;

/// <summary>
/// Integration event raised when a presentation is updated during conference import/synchronization.
/// This event is published per presentation when at least one field has changed.
/// </summary>
public sealed class PresentationUpdatedEvent : IntegrationEvent
{
    public override string EventType => "presentation.updated";

    /// <summary>
    /// Gets the ID of the conference this presentation belongs to.
    /// </summary>
    public Guid ConferenceId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the presentation.
    /// </summary>
    public Guid PresentationId { get; init; }

    /// <summary>
    /// Gets the title of the presentation.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the abstract/description of the presentation.
    /// </summary>
    public string Abstract { get; init; } = string.Empty;

    /// <summary>
    /// Gets the start date and time of the presentation.
    /// </summary>
    public DateTime StartDateTime { get; init; }

    /// <summary>
    /// Gets the end date and time of the presentation.
    /// </summary>
    public DateTime EndDateTime { get; init; }

    /// <summary>
    /// Gets the ID of the room where the presentation is held.
    /// </summary>
    public Guid RoomId { get; init; }

    /// <summary>
    /// Gets the name of the room where the presentation is held.
    /// </summary>
    public string RoomName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the collection of speaker IDs for this presentation.
    /// </summary>
    public IReadOnlyCollection<Guid> SpeakerIds { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets the external ID from the synchronization source (e.g., Sessionize).
    /// </summary>
    public string? ExternalId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the schedule has changed.
    /// This is true when the date/time changed or the presentation was moved to a different room.
    /// </summary>
    public bool IsScheduleChanged { get; init; }
}
