namespace HexMaster.Attendr.Core.DomainModels;

/// <summary>
/// Example domain model demonstrating how to use the StatefulDomainModel base class.
/// This example shows proper usage of the state machine in property setters.
/// </summary>
/// <remarks>
/// This is a reference implementation. Actual domain models in the system
/// can follow this pattern for proper state tracking.
/// </remarks>
public sealed class ExampleStatefulEntity : StatefulDomainModel<Guid>
{
    private string _name;
    private string? _description;
    private int _value;

    /// <summary>
    /// Gets the name of the entity.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Gets the optional description of the entity.
    /// </summary>
    public string? Description => _description;

    /// <summary>
    /// Gets the value of the entity.
    /// </summary>
    public int Value => _value;

    /// <summary>
    /// Private constructor for ORM/serialization.
    /// </summary>
    private ExampleStatefulEntity() : base()
    {
    }

    /// <summary>
    /// Creates a new entity with Created state (e.g., from a POST request).
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The name of the entity.</param>
    /// <param name="description">The optional description.</param>
    /// <param name="value">The value.</param>
    private ExampleStatefulEntity(Guid id, string name, string? description, int value)
        : base(id, DomainModelState.Created)
    {
        _name = name;
        _description = description;
        _value = value;
    }

    /// <summary>
    /// Loads an existing entity from the database with Pristine state.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The name of the entity.</param>
    /// <param name="description">The optional description.</param>
    /// <param name="value">The value.</param>
    /// <param name="createdOn">The creation date.</param>
    /// <param name="modifiedOn">The last modification date.</param>
    private ExampleStatefulEntity(
        Guid id,
        string name,
        string? description,
        int value,
        DateTimeOffset createdOn,
        DateTimeOffset? modifiedOn)
        : base(id, DomainModelState.Pristine)
    {
        _name = name;
        _description = description;
        _value = value;
        SetCreatedOn(createdOn);
        SetModifiedOn(modifiedOn);
    }

    /// <summary>
    /// Factory method to create a new entity (Created state).
    /// </summary>
    public static ExampleStatefulEntity Create(Guid id, string name, string? description = null, int value = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        return new ExampleStatefulEntity(id, name, description, value);
    }

    /// <summary>
    /// Factory method to load an existing entity from database (Pristine state).
    /// </summary>
    public static ExampleStatefulEntity Load(
        Guid id,
        string name,
        string? description,
        int value,
        DateTimeOffset createdOn,
        DateTimeOffset? modifiedOn)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        return new ExampleStatefulEntity(id, name, description, value, createdOn, modifiedOn);
    }

    /// <summary>
    /// Updates the name using the ShouldUpdateProperty helper method.
    /// Demonstrates the recommended approach for property setters.
    /// </summary>
    /// <param name="name">The new name.</param>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if (ShouldUpdateProperty(_name, name))
        {
            _name = name;
            UpdateModifiedOn();
        }
    }

    /// <summary>
    /// Updates the description using manual state tracking.
    /// Demonstrates the alternative approach for property setters.
    /// </summary>
    /// <param name="description">The new description.</param>
    public void UpdateDescription(string? description)
    {
        TrackPropertyChange(); // Mark as Touched

        if (!Equals(_description, description))
        {
            _description = description;
            SetState(DomainModelState.Modified); // Only Modified if value actually changed
            UpdateModifiedOn();
        }
    }

    /// <summary>
    /// Updates the value demonstrating the pattern with value types.
    /// </summary>
    /// <param name="value">The new value.</param>
    public void UpdateValue(int value)
    {
        if (ShouldUpdateProperty(_value, value))
        {
            _value = value;
            UpdateModifiedOn();
        }
    }

    /// <summary>
    /// Marks the entity for deletion.
    /// </summary>
    public void MarkAsDeleted()
    {
        SetState(DomainModelState.Deleted);
    }
}
