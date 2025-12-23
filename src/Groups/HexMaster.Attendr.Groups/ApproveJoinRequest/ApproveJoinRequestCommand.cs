using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Groups.ApproveJoinRequest;

/// <summary>
/// Command to approve a join request for a group.
/// The requesting member will be added to the group with Member role.
/// </summary>
/// <param name="GroupId">The ID of the group.</param>
/// <param name="ProfileIdToApprove">The ID of the profile whose join request should be approved.</param>
/// <param name="RequestingProfileId">The ID of the profile making the approval (must be Owner or Manager).</param>
public sealed record ApproveJoinRequestCommand(
    Guid GroupId,
    Guid ProfileIdToApprove,
    Guid RequestingProfileId) : IAttendrCommand;
