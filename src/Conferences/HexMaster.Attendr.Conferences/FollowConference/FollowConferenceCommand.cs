namespace HexMaster.Attendr.Conferences.FollowConference;

/// <summary>
/// Command to follow a conference for a specific profile.
/// </summary>
/// <param name="ConferenceId">The unique identifier of the conference to follow.</param>
/// <param name="ProfileId">The unique identifier of the profile following the conference.</param>
public sealed record FollowConferenceCommand(
    Guid ConferenceId,
    Guid ProfileId);
