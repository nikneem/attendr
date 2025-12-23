using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Groups.JoinGroup;

/// <summary>
/// Command to join a group.
/// If the group is public, the member is added directly.
/// If the group is private, a join request is created.
/// </summary>
/// <param name="GroupId">The ID of the group to join.</param>
/// <param name="ProfileId">The ID of the profile requesting to join.</param>
/// <param name="ProfileName">The name of the profile requesting to join.</param>
public sealed record JoinGroupCommand(
    Guid GroupId,
    Guid ProfileId,
    string ProfileName) : IAttendrCommand;
