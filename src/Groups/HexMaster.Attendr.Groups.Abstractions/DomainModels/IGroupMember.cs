namespace HexMaster.Attendr.Groups.Abstractions.DomainModels;

/// <summary>
/// Interface representing a member of a group.
/// </summary>
public interface IGroupMember
{
    /// <summary>
    /// Gets the unique identifier of the member.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the role of the member in the group.
    /// </summary>
    GroupRole Role { get; }

    /// <summary>
    /// Updates the role of the member.
    /// </summary>
    /// <param name="newRole">The new role for the member.</param>
    void UpdateRole(GroupRole newRole);

    /// <summary>
    /// Updates the name of the member.
    /// </summary>
    /// <param name="newName">The new name for the member.</param>
    void UpdateName(string newName);
}
