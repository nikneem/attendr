namespace HexMaster.Attendr.Groups.Abstractions.Dtos;

/// <summary>
/// Request DTO for updating a member's role in a group.
/// </summary>
/// <param name="Role">The new role for the member (0=Owner, 1=Manager, 2=Member).</param>
public sealed record UpdateMemberRoleRequest(GroupRole Role);
