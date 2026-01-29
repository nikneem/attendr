namespace HexMaster.Attendr.Core.DomainEvents;

/// <summary>
/// Base record for all domain events.
/// Domain events represent something significant that happened in the domain.
/// </summary>
public abstract record DomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this domain event.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the date and time when this event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
