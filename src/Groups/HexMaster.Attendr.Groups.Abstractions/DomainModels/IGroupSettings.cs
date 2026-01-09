namespace HexMaster.Attendr.Groups.Abstractions.DomainModels;

/// <summary>
/// Interface representing settings for a group.
/// </summary>
public interface IGroupSettings
{
    /// <summary>
    /// Gets a value indicating whether the group is public and visible to all users.
    /// </summary>
    bool IsPublic { get; }

    /// <summary>
    /// Gets a value indicating whether the group is searchable by other attendees.
    /// </summary>
    bool IsSearchable { get; }
}
