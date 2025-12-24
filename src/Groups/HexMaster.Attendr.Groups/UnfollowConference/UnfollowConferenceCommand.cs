namespace HexMaster.Attendr.Groups.UnfollowConference;

/// <summary>
/// Command to unfollow a conference in a group.
/// </summary>
/// <param name="GroupId">The unique identifier of the group.</param>
/// <param name="ConferenceId">The unique identifier of the conference to unfollow.</param>
/// <param name="RequestingProfileId">The unique identifier of the profile requesting the action.</param>
public sealed record UnfollowConferenceCommand(
    Guid GroupId,
    Guid ConferenceId,
    Guid RequestingProfileId);
