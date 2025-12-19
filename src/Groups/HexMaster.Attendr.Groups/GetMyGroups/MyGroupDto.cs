namespace HexMaster.Attendr.Groups.GetMyGroups;

/// <summary>
/// DTO representing a group in the user's membership list.
/// </summary>
public sealed record MyGroupDto(
    Guid Id,
    string Name,
    int MemberCount);
