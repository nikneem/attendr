namespace HexMaster.Attendr.Presence.Features.GetConferenceAttendance;

/// <summary>
/// Query to retrieve conference attendance information for a profile.
/// </summary>
/// <param name="ProfileId">The unique identifier of the profile.</param>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
public sealed record GetConferenceAttendanceQuery(Guid ProfileId, Guid ConferenceId);
