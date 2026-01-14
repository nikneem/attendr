using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;

namespace HexMaster.Attendr.Groups.Features.UpdateMemberRole;

/// <summary>
/// Command to update a member's role in a group.
/// Only group owners can change member roles.
/// </summary>
/// <param name="GroupId">The ID of the group.</param>
/// <param name="MemberId">The ID of the member whose role is being updated.</param>
/// <param name="NewRole">The new role for the member.</param>
/// <param name="RequestingProfileId">The ID of the profile making the request (must be owner).</param>
public sealed record UpdateMemberRoleCommand(
    Guid GroupId,
    Guid MemberId,
    GroupRole NewRole,
    Guid RequestingProfileId) : IAttendrCommand;
