namespace HexMaster.Attendr.Groups.Abstractions.Dtos;

/// <summary>
/// Defines the role a member can have within a group.
/// </summary>
public enum GroupRole
{
    /// <summary>
    /// Owner of the group with full control.
    /// </summary>
    Owner = 0,

    /// <summary>
    /// Manager with administrative privileges.
    /// </summary>
    Manager = 1,

    /// <summary>
    /// Regular member of the group.
    /// </summary>
    Member = 2
}
