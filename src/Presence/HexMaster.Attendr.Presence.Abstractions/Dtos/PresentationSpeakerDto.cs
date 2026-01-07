namespace HexMaster.Attendr.Presence.Abstractions.Dtos;

/// <summary>
/// DTO containing speaker information for a presentation.
/// </summary>
/// <param name="SpeakerId">The unique identifier of the speaker.</param>
/// <param name="Name">The name of the speaker.</param>
/// <param name="ProfilePictureUrl">The URL to the speaker's profile picture.</param>
public record PresentationSpeakerDto(
    Guid SpeakerId,
    string Name,
    string? ProfilePictureUrl
);
