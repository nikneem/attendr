namespace HexMaster.Attendr.Presence.Api.Features.GetMyConferences;

/// <summary>
/// Response model for user's conferences.
/// </summary>
public sealed record MyConferenceResponse(
    Guid ConferenceId,
    string ConferenceName,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAttending);
