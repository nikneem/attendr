namespace HexMaster.Attendr.Groups.GetGroupFollowedConferences;

/// <summary>
/// Query to get all followed conferences for a group.
/// Only returns future conferences.
/// </summary>
/// <param name="GroupId">The unique identifier of the group.</param>
/// <param name="RequestingProfileId">The unique identifier of the profile requesting the information.</param>
public sealed record GetGroupFollowedConferencesQuery(
    Guid GroupId,
    Guid RequestingProfileId);
