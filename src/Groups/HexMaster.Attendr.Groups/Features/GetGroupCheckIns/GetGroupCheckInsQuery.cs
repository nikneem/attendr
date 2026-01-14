namespace HexMaster.Attendr.Groups.Features.GetGroupCheckIns;

/// <summary>
/// Query to retrieve all active check-ins for a specific group.
/// </summary>
/// <param name="GroupId">The group identifier.</param>
public sealed record GetGroupCheckInsQuery(Guid GroupId);
