namespace HexMaster.Attendr.Groups.GetGroupDetails;

using HexMaster.Attendr.Groups.DomainModels;

/// <summary>
/// DTO representing detailed information about a group.
/// </summary>
/// <param name="Id">The unique identifier of the group.</param>
/// <param name="Name">The name of the group.</param>
/// <param name="MemberCount">The total number of members in the group.</param>
/// <param name="IsMember">Indicates whether the current profile is a member of the group.</param>
/// <param name="IsPublic">Indicates whether the group is public (anyone can join) or private (requires approval).</param>
/// <param name="CurrentMemberRole">The role of the current member in the group, null if not a member.</param>
/// <param name="Members">The list of members in the group.</param>
/// <param name="Invitations">The list of pending invitations.</param>
/// <param name="JoinRequests">The list of pending join requests.</param>
public sealed record GroupDetailsDto(
    Guid Id,
    string Name,
    int MemberCount,
    bool IsMember,
    bool IsPublic,
    GroupRole? CurrentMemberRole,
    List<GetGroupDetailsMemberDto> Members,
    List<GroupInvitationDto> Invitations,
    List<GroupJoinRequestDto> JoinRequests);
