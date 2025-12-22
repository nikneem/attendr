namespace HexMaster.Attendr.Groups.GetGroupDetails;

/// <summary>
/// DTO representing detailed information about a group.
/// </summary>
/// <param name="Id">The unique identifier of the group.</param>
/// <param name="Name">The name of the group.</param>
/// <param name="MemberCount">The total number of members in the group.</param>
/// <param name="IsMember">Indicates whether the current profile is a member of the group.</param>
/// <param name="IsPublic">Indicates whether the group is public (anyone can join) or private (requires approval).</param>
public sealed record GroupDetailsDto(
    Guid Id,
    string Name,
    int MemberCount,
    bool IsMember,
    bool IsPublic);
