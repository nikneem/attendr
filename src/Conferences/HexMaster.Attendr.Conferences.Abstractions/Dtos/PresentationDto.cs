namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

/// <summary>
/// DTO for presentation information.
/// </summary>
/// <param name="Id">The presentation identifier.</param>
/// <param name="Title">The title of the presentation.</param>
/// <param name="Abstract">The abstract of the presentation.</param>
/// <param name="StartDateTime">The start date and time of the presentation.</param>
/// <param name="EndDateTime">The end date and time of the presentation.</param>
/// <param name="RoomName">The name of the room where the presentation is held.</param>
/// <param name="Speakers">The list of speakers for this presentation.</param>
public sealed record PresentationDto(
    Guid Id,
    string Title,
    string Abstract,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string RoomName,
    List<SpeakerDto> Speakers);
