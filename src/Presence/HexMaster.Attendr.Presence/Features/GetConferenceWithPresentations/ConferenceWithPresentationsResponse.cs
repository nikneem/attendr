namespace HexMaster.Attendr.Presence.Features.GetConferenceWithPresentations;

/// <summary>
/// Response model containing conference details with presentation presence information.
/// </summary>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
/// <param name="ConferenceName">The name of the conference.</param>
/// <param name="Location">The location of the conference.</param>
/// <param name="ImageUrl">Optional image URL for the conference.</param>
/// <param name="StartDate">The start date of the conference.</param>
/// <param name="EndDate">The end date of the conference.</param>
/// <param name="IsFollowing">Indicates whether the profile is following this conference.</param>
/// <param name="IsAttending">Indicates whether the profile is attending this conference.</param>
/// <param name="Presentations">Collection of presentations with presence information.</param>
public sealed record ConferenceWithPresentationsResponse(
    Guid ConferenceId,
    string ConferenceName,
    string Location,
    string? ImageUrl,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsFollowing,
    bool IsAttending,
    IReadOnlyCollection<PresentationPresenceResponse> Presentations);

/// <summary>
/// Response model for presentation with presence information.
/// </summary>
/// <param name="PresentationId">The unique identifier of the presentation.</param>
/// <param name="Title">The title of the presentation.</param>
/// <param name="Abstract">The abstract/description of the presentation.</param>
/// <param name="Room">The room where the presentation is held.</param>
/// <param name="StartDateTime">The start date and time of the presentation.</param>
/// <param name="EndDateTime">The end date and time of the presentation.</param>
/// <param name="Speakers">Collection of speakers for the presentation.</param>
/// <param name="IsFavorite">Indicates whether this presentation is marked as favorite.</param>
/// <param name="IsRecommended">Indicates whether this presentation is recommended.</param>
/// <param name="IsPreferred">Indicates whether this presentation is preferred.</param>
/// <param name="IsRated">Indicates whether this presentation has been rated.</param>
/// <param name="IsCheckedIn">Indicates whether the user is checked in to this presentation.</param>
/// <param name="Rating">The rating given to the presentation (0-5), if any.</param>
public sealed record PresentationPresenceResponse(
    Guid PresentationId,
    string Title,
    string Abstract,
    string Room,
    DateTime StartDateTime,
    DateTime EndDateTime,
    IReadOnlyCollection<SpeakerResponse> Speakers,
    bool IsFavorite,
    bool IsRecommended,
    bool IsPreferred,
    bool IsRated,
    bool IsCheckedIn,
    byte? Rating);

/// <summary>
/// Response model for speaker information.
/// </summary>
/// <param name="SpeakerId">The unique identifier of the speaker.</param>
/// <param name="Name">The name of the speaker.</param>
/// <param name="ProfilePictureUrl">Optional profile picture URL for the speaker.</param>
public sealed record SpeakerResponse(
    Guid SpeakerId,
    string Name,
    string? ProfilePictureUrl);
