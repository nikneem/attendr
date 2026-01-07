namespace HexMaster.Attendr.Presence.Features.CheckIn;

/// <summary>
/// Command to check in or out of a presentation.
/// </summary>
/// <param name="ProfileId">The unique identifier of the profile.</param>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
/// <param name="PresentationId">The unique identifier of the presentation.</param>
/// <param name="IsCheckedIn">True to check in, false to check out.</param>
public sealed record CheckInCommand(
    Guid ProfileId,
    Guid ConferenceId,
    Guid PresentationId,
    bool IsCheckedIn);
