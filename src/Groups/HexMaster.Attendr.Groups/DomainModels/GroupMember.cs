namespace HexMaster.Attendr.Groups.DomainModels;

/// <summary>
/// Represents a member of a group.
/// Follows Domain-Driven Design principles with encapsulated properties.
/// </summary>
public sealed class GroupMember
{
    /// <summary>
    /// Gets the unique identifier of the member.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the role of the member in the group.
    /// </summary>
    public GroupRole Role { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupMember"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the member.</param>
    /// <param name="name">The name of the member.</param>
    /// <param name="role">The role of the member in the group.</param>
    /// <exception cref="ArgumentException">Thrown when ID is empty or name is null/whitespace.</exception>
    public GroupMember(Guid id, string name, GroupRole role)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Member ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Member name cannot be empty.", nameof(name));
        }

        Id = id;
        Name = name;
        Role = role;
    }

    /// <summary>
    /// Updates the role of the member.
    /// </summary>
    /// <param name="newRole">The new role for the member.</param>
    public void UpdateRole(GroupRole newRole)
    {
        Role = newRole;
    }

    /// <summary>
    /// Updates the name of the member.
    /// </summary>
    /// <param name="newName">The new name for the member.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Member name cannot be empty.", nameof(newName));
        }

        Name = newName;
    }
}
