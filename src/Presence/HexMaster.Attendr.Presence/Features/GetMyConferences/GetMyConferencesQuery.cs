namespace HexMaster.Attendr.Presence.Features.GetMyConferences;

/// <summary>
/// Query to retrieve all current and future conferences for a profile.
/// </summary>
/// <param name="ProfileId">The unique identifier of the profile.</param>
public sealed record GetMyConferencesQuery(Guid ProfileId);
