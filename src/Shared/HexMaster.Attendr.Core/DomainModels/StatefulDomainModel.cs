using HexMaster.Attendr.Core.DomainEvents;

namespace HexMaster.Attendr.Core.DomainModels;

/// <summary>
/// Base class for domain models with state tracking following Domain-Driven Design principles.
/// Implements a state machine to track the lifecycle of domain objects.
/// </summary>
/// <typeparam name="TId">The type of the domain model identifier.</typeparam>
public abstract class StatefulDomainModel<TId> : IDomainEventContainer
    where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the immutable unique identifier of the domain model.
    /// </summary>
    public TId Id { get; private set; }

    /// <summary>
    /// Gets the current state of the domain model in its lifecycle.
    /// </summary>
    public DomainModelState State { get; private set; }

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
    /// Initializes a new instance of the <see cref="StatefulDomainModel{TId}"/> class.
    /// Protected parameterless constructor for ORM/serialization support.
    /// Objects created this way should call SetId and SetState explicitly.
    /// </summary>
    protected StatefulDomainModel()
    {
        Id = default!;
        State = DomainModelState.Pristine;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StatefulDomainModel{TId}"/> class with an identifier and initial state.
    /// </summary>
    /// <param name="id">The immutable unique identifier for the domain model.</param>
    /// <param name="initialState">The initial state of the domain model. Must be either Created or Pristine. Defaults to Pristine.</param>
    /// <exception cref="ArgumentNullException">Thrown when id is null.</exception>
    /// <exception cref="ArgumentException">Thrown when initialState is not Created or Pristine.</exception>
    protected StatefulDomainModel(TId id, DomainModelState initialState = DomainModelState.Pristine)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id), "Id cannot be null.");
        }

        if (initialState != DomainModelState.Created && initialState != DomainModelState.Pristine)
        {
            throw new ArgumentException("Initial state must be either Created or Pristine.", nameof(initialState));
        }

        Id = id;
        State = initialState;
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
    /// Attempts to change the state of the domain model according to state machine rules.
    /// If the state transition is not allowed, the call is silently ignored.
    /// </summary>
    /// <param name="newState">The desired new state.</param>
    /// <remarks>
    /// State transition rules:
    /// - Created: Cannot change state (terminal state for new objects)
    /// - Objects can never transition TO Created (except during construction)
    /// - Pristine: Can change to Touched, Modified, or Deleted
    /// - Touched: Can change to Modified or Deleted
    /// - Modified: Can only change to Deleted
    /// - Deleted: Cannot change state (terminal state)
    /// </remarks>
    protected void SetState(DomainModelState newState)
    {
        // Cannot transition to the same state
        if (State == newState)
        {
            return;
        }

        // Cannot transition TO Created (only initial state)
        if (newState == DomainModelState.Created)
        {
            return;
        }

        // Check if transition is allowed based on current state
        var isAllowed = State switch
        {
            DomainModelState.Created => false, // Terminal state - no transitions allowed
            DomainModelState.Pristine => newState is DomainModelState.Touched or DomainModelState.Modified or DomainModelState.Deleted,
            DomainModelState.Touched => newState is DomainModelState.Modified or DomainModelState.Deleted,
            DomainModelState.Modified => newState is DomainModelState.Deleted,
            DomainModelState.Deleted => false, // Terminal state - no transitions allowed
            _ => false
        };

        if (isAllowed)
        {
            State = newState;
        }
    }

    /// <summary>
    /// Helper method for property setters to track state changes properly.
    /// Call this before checking if the value actually changed.
    /// </summary>
    /// <remarks>
    /// Usage pattern in derived classes:
    /// <code>
    /// public void SetProperty(string newValue)
    /// {
    ///     TrackPropertyChange(); // Marks as Touched
    ///     if (!Equals(_property, newValue))
    ///     {
    ///         _property = newValue;
    ///         SetState(DomainModelState.Modified); // Only Modified if value actually changed
    ///     }
    /// }
    /// </code>
    /// </remarks>
    protected void TrackPropertyChange()
    {
        SetState(DomainModelState.Touched);
    }

    /// <summary>
    /// Helper method for property setters that combines state tracking with value comparison.
    /// Returns true if the value actually changed and the property should be updated.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <returns>True if the value changed and should be updated; otherwise false.</returns>
    /// <remarks>
    /// Usage pattern in derived classes:
    /// <code>
    /// public void SetProperty(string newValue)
    /// {
    ///     if (ShouldUpdateProperty(_property, newValue))
    ///     {
    ///         _property = newValue;
    ///     }
    /// }
    /// </code>
    /// </remarks>
    protected bool ShouldUpdateProperty<TValue>(TValue currentValue, TValue newValue)
    {
        TrackPropertyChange();

        if (Equals(currentValue, newValue))
        {
            return false;
        }

        SetState(DomainModelState.Modified);
        return true;
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
