namespace HexMaster.Attendr.Presence.Features.GetCurrentConferences;

/// <summary>
/// Query to retrieve all current conferences the profile is following and attending.
/// </summary>
public sealed record GetCurrentConferencesQuery(Guid ProfileId);
