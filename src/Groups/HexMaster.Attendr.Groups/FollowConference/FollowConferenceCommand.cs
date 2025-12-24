namespace HexMaster.Attendr.Groups.FollowConference;

/// <summary>
/// Command to follow a conference in a group.
/// </summary>
/// <param name="GroupId">The unique identifier of the group.</param>
/// <param name="ConferenceId">The unique identifier of the conference to follow.</param>
/// <param name="RequestingProfileId">The unique identifier of the profile requesting the action.</param>
public sealed record FollowConferenceCommand(
    Guid GroupId,
    Guid ConferenceId,
    Guid RequestingProfileId);
