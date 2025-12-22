namespace HexMaster.Attendr.Groups.GetGroupDetails;

/// <summary>
/// Query to retrieve detailed information about a specific group.
/// </summary>
/// <param name="GroupId">The unique identifier of the group.</param>
/// <param name="ProfileId">The ID of the profile making the request.</param>
public sealed record GetGroupDetailsQuery(
    Guid GroupId,
    Guid ProfileId);
