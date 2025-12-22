namespace HexMaster.Attendr.Groups.DomainModels;

/// <summary>
/// Represents a request from a profile to join a group.
/// </summary>
public sealed class GroupJoinRequest
{
    /// <summary>
    /// Gets the unique identifier of the requester.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the name of the requester.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the date and time when the request was made.
    /// </summary>
    public DateTimeOffset RequestedAt { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupJoinRequest"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the requester.</param>
    /// <param name="name">The name of the requester.</param>
    /// <param name="requestedAt">The date and time when the request was made.</param>
    public GroupJoinRequest(Guid id, string name, DateTimeOffset requestedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Requester ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Requester name cannot be empty.", nameof(name));
        }

        Id = id;
        Name = name;
        RequestedAt = requestedAt;
    }
}
