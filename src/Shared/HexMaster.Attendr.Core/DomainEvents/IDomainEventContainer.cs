namespace HexMaster.Attendr.Core.DomainEvents;

/// <summary>
/// Interface for domain models that can raise domain events.
/// </summary>
public interface IDomainEventContainer
{
    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from the entity.
    /// This is typically called after events have been dispatched.
    /// </summary>
    void ClearDomainEvents();
}
