namespace HexMaster.Attendr.Presence.Abstractions.Dtos;

/// <summary>
/// DTO containing presentation information for rating.
/// </summary>
/// <param name="PresentationId">The unique identifier of the presentation.</param>
/// <param name="Title">The title of the presentation.</param>
/// <param name="Abstract">The abstract/description of the presentation.</param>
/// <param name="Room">The name of the room where the presentation is held.</param>
/// <param name="StartDateTime">The start date and time of the presentation.</param>
/// <param name="EndDateTime">The end date and time of the presentation.</param>
public record PresentationToRateDto(
    Guid PresentationId,
    string Title,
    string Abstract,
    string Room,
    DateTime StartDateTime,
    DateTime EndDateTime
);
