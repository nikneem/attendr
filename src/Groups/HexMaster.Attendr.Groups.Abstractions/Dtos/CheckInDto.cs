namespace HexMaster.Attendr.Groups.Abstractions.Dtos;

/// <summary>
/// DTO representing a group check-in to a presentation.
/// </summary>
/// <param name="Id">The unique identifier for the check-in.</param>
/// <param name="GroupId">The group identifier.</param>
/// <param name="ConferenceId">The conference identifier.</param>
/// <param name="PresentationId">The presentation identifier.</param>
/// <param name="PresentationData">The presentation data.</param>
/// <param name="Members">The collection of checked-in members.</param>
/// <param name="Expiration">The expiration date/time for the check-in.</param>
public sealed record CheckInDto(
    Guid Id,
    Guid GroupId,
    Guid ConferenceId,
    Guid PresentationId,
    CheckInPresentationDataDto PresentationData,
    List<CheckedInMemberDto> Members,
    DateTimeOffset Expiration);

/// <summary>
/// DTO representing presentation data in a check-in.
/// </summary>
/// <param name="Id">The presentation identifier.</param>
/// <param name="Title">The presentation title.</param>
/// <param name="Abstract">The presentation abstract.</param>
/// <param name="Room">The room where the presentation takes place.</param>
/// <param name="StartDateTime">The start date and time of the presentation.</param>
/// <param name="EndDateTime">The end date and time of the presentation.</param>
/// <param name="Speakers">The list of speakers.</param>
public sealed record CheckInPresentationDataDto(
    Guid Id,
    string Title,
    string Abstract,
    string Room,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    List<CheckInPresentationSpeakerDto> Speakers);

/// <summary>
/// DTO representing a speaker in a presentation.
/// </summary>
/// <param name="Id">The speaker identifier.</param>
/// <param name="Name">The speaker name.</param>
/// <param name="ProfilePictureUrl">The URL to the speaker's profile picture.</param>
public sealed record CheckInPresentationSpeakerDto(
    Guid Id,
    string Name,
    string? ProfilePictureUrl);

/// <summary>
/// DTO representing a member who is checked in.
/// </summary>
/// <param name="Id">The member identifier.</param>
/// <param name="Name">The member name.</param>
/// <param name="ProfilePictureUrl">The URL to the member's profile picture.</param>
public sealed record CheckedInMemberDto(
    Guid Id,
    string Name,
    string? ProfilePictureUrl);
