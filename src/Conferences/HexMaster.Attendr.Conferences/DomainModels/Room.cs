namespace HexMaster.Attendr.Conferences.DomainModels;

/// <summary>
/// Represents a room at a conference venue.
/// </summary>
public sealed class Room
{
    /// <summary>
    /// Gets the unique identifier of the room.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the name of the room.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the external ID from the synchronization source.
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>
    /// Gets the capacity of the room.
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Room"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the room.</param>
    /// <param name="name">The name of the room.</param>
    /// <param name="capacity">The capacity of the room.</param>
    /// <param name="externalId">The external ID from the synchronization source.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public Room(Guid id, string name, int capacity, string? externalId = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Room ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Room name cannot be empty.", nameof(name));
        }

        if (capacity <= 0)
        {
            throw new ArgumentException("Room capacity must be greater than zero.", nameof(capacity));
        }

        Id = id;
        Name = name;
        Capacity = capacity;
        ExternalId = externalId;
    }

    /// <summary>
    /// Factory method to create a new room.
    /// </summary>
    /// <param name="name">The name of the room.</param>
    /// <param name="capacity">The capacity of the room.</param>
    /// <param name="externalId">The external ID from the synchronization source.</param>
    /// <returns>A new instance of <see cref="Room"/>.</returns>
    public static Room Create(string name, int capacity, string? externalId = null)
    {
        var id = Guid.NewGuid();
        return new Room(id, name, capacity, externalId);
    }
}
