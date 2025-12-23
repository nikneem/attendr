using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Groups.RemoveMember;

/// <summary>
/// Command to remove a member from a group.
/// Only owners and managers can remove members.
/// The last owner cannot be removed.
/// </summary>
/// <param name="GroupId">The ID of the group.</param>
/// <param name="MemberIdToRemove">The ID of the member to remove.</param>
/// <param name="RequestingProfileId">The ID of the profile making the request.</param>
public sealed record RemoveMemberCommand(
    Guid GroupId,
    Guid MemberIdToRemove,
    Guid RequestingProfileId) : IAttendrCommand;
