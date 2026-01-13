namespace HexMaster.Attendr.Presence.Features.GetConferenceScheduleNow;

/// <summary>
/// Query to retrieve the profile's favorite presentations for a conference organized by timeslots (Previous, Now, Next).
/// </summary>
/// <param name="ProfileId">The unique identifier of the profile.</param>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
public sealed record GetConferenceScheduleNowQuery(
    Guid ProfileId,
    Guid ConferenceId);
