namespace HexMaster.Attendr.Groups.Features.GetMyGroups;

/// <summary>
/// Query to retrieve all groups where the current user is a member.
/// </summary>
public sealed record GetMyGroupsQuery(Guid ProfileId);

