namespace HexMaster.Attendr.Presence.Features.GetConferenceScheduleNow;

/// <summary>
/// Response containing the profile's favorite presentations organized by timeslot (Previous, Now, Next).
/// </summary>
/// <param name="Previous">Favorite presentations from the previous timeslot.</param>
/// <param name="Now">Favorite presentations currently running.</param>
/// <param name="Next">Favorite presentations in the next timeslot.</param>
public sealed record ConferenceScheduleNowResponse(
    IReadOnlyCollection<ScheduledPresentationResponse> Previous,
    IReadOnlyCollection<ScheduledPresentationResponse> Now,
    IReadOnlyCollection<ScheduledPresentationResponse> Next);

/// <summary>
/// Response model for a scheduled presentation.
/// </summary>
/// <param name="PresentationId">The unique identifier of the presentation.</param>
/// <param name="Title">The title of the presentation.</param>
/// <param name="Abstract">The abstract/description of the presentation.</param>
/// <param name="Room">The room where the presentation is held.</param>
/// <param name="StartDateTime">The start date and time of the presentation.</param>
/// <param name="EndDateTime">The end date and time of the presentation.</param>
/// <param name="Speakers">Collection of speakers for the presentation.</param>
/// <param name="IsPreferred">Indicates whether this presentation is the preferred choice for its timeslot.</param>
public sealed record ScheduledPresentationResponse(
    Guid PresentationId,
    string Title,
    string Abstract,
    string Room,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    IReadOnlyCollection<ScheduledSpeakerResponse> Speakers,
    bool IsPreferred);

/// <summary>
/// Response model for speaker information in a scheduled presentation.
/// </summary>
/// <param name="SpeakerId">The unique identifier of the speaker.</param>
/// <param name="Name">The name of the speaker.</param>
/// <param name="ProfilePictureUrl">Optional URL to the speaker's profile picture.</param>
public sealed record ScheduledSpeakerResponse(
    Guid SpeakerId,
    string Name,
    string? ProfilePictureUrl);
