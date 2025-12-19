namespace HexMaster.Attendr.Groups.ListGroups;

/// <summary>
/// DTO representing a group in a list view with membership information.
/// </summary>
/// <param name="Id">The unique identifier of the group.</param>
/// <param name="Name">The name of the group.</param>
/// <param name="MemberCount">The total number of members in the group.</param>
/// <param name="IsMember">Indicates whether the current profile is a member of the group.</param>
public sealed record GroupListItemDto(
    Guid Id,
    string Name,
    int MemberCount,
    bool IsMember);
