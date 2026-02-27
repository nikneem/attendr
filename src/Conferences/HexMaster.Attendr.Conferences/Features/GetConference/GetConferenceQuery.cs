namespace HexMaster.Attendr.Conferences.Features.GetConference;

/// <summary>
/// Query to retrieve a specific conference by ID.
/// </summary>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
/// <param name="CurrentProfileId">The profile ID of the requesting user, used to apply owner visibility override.</param>
/// <param name="IsAdmin">Whether the requesting user has admin privileges, used to bypass visibility restrictions.</param>
public sealed record GetConferenceQuery(Guid ConferenceId, Guid? CurrentProfileId = null, bool IsAdmin = false);
