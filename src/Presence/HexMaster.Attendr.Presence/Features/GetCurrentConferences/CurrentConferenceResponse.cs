namespace HexMaster.Attendr.Presence.Features.GetCurrentConferences;

/// <summary>
/// Response model for current conferences the profile is following and attending.
/// </summary>
public sealed record CurrentConferenceResponse(
    Guid ConferenceId,
    string ConferenceName,
    string Location,
    string? ImageUrl,
    DateTime StartDate,
    DateTime EndDate);
