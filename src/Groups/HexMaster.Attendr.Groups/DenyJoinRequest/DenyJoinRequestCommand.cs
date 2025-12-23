using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Groups.DenyJoinRequest;

/// <summary>
/// Command to deny a join request for a group.
/// The join request will be removed from the group.
/// </summary>
/// <param name="GroupId">The ID of the group.</param>
/// <param name="ProfileIdToDeny">The ID of the profile whose join request should be denied.</param>
/// <param name="RequestingProfileId">The ID of the profile making the denial (must be Owner or Manager).</param>
public sealed record DenyJoinRequestCommand(
    Guid GroupId,
    Guid ProfileIdToDeny,
    Guid RequestingProfileId) : IAttendrCommand;
