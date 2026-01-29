using HexMaster.Attendr.Core.DomainEvents;

namespace HexMaster.Attendr.Core.DomainModels;

/// <summary>
/// Base class for domain models following Domain-Driven Design principles.
/// Provides common functionality and enforces DDD patterns such as private setters
/// and validation through behavior methods.
/// </summary>
/// <typeparam name="TId">The type of the domain model identifier.</typeparam>
public abstract class DomainModel<TId> : IDomainEventContainer
    where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the immutable unique identifier of the domain model.
    /// </summary>
    public TId Id { get; private set; }

    /// <summary>
    /// Gets the date and time when the domain model was created.
    /// </summary>
    public DateTimeOffset CreatedOn { get; private set; }

    /// <summary>
    /// Gets the date and time when the domain model was last modified.
    /// </summary>
    public DateTimeOffset? ModifiedOn { get; private set; }

    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainModel{TId}"/> class.
    /// Protected parameterless constructor for ORM/serialization support.
    /// </summary>
    protected DomainModel()
    {
        Id = default!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainModel{TId}"/> class with an identifier.
    /// </summary>
    /// <param name="id">The immutable unique identifier for the domain model.</param>
    /// <exception cref="ArgumentNullException">Thrown when id is null.</exception>
    protected DomainModel(TId id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id), "Id cannot be null.");
        }

        Id = id;
        CreatedOn = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Sets the domain model identifier. This should only be called internally during initialization or deserialization.
    /// </summary>
    /// <param name="id">The immutable unique identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown when id is null.</exception>
    protected void SetId(TId id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id), "Id cannot be null.");
        }

        Id = id;
    }

    /// <summary>
    /// Sets the created date. Typically called when loading from persistence.
    /// </summary>
    /// <param name="createdOn">The creation date.</param>
    protected void SetCreatedOn(DateTimeOffset createdOn)
    {
        CreatedOn = createdOn;
    }

    /// <summary>
    /// Sets the modified date to the current UTC time.
    /// </summary>
    protected void UpdateModifiedOn()
    {
        ModifiedOn = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Sets the modified date to a specific date. Typically called when loading from persistence.
    /// </summary>
    /// <param name="modifiedOn">The modification date.</param>
    protected void SetModifiedOn(DateTimeOffset? modifiedOn)
    {
        ModifiedOn = modifiedOn;
    }

    /// <summary>
    /// Adds a domain event to the entity's event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the entity.
    /// This is typically called after events have been dispatched.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
