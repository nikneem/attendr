namespace HexMaster.Attendr.Presence.Features.GetMyConferences;

/// <summary>
/// Response model for user's conferences.
/// </summary>
public sealed record MyConferenceResponse(
    Guid ConferenceId,
    string ConferenceName,
    string Location,
    string? ImageUrl,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAttending);
