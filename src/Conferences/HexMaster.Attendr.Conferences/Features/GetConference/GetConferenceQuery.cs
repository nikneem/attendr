namespace HexMaster.Attendr.Conferences.Features.GetConference;

/// <summary>
/// Query to retrieve a specific conference by ID.
/// </summary>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
public sealed record GetConferenceQuery(Guid ConferenceId);
