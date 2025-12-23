namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

/// <summary>
/// DTO for speaker information.
/// </summary>
/// <param name="Id">The speaker identifier.</param>
/// <param name="Name">The name of the speaker.</param>
/// <param name="ProfilePictureUrl">Optional URL to the speaker's profile picture.</param>
public sealed record SpeakerDto(
    Guid Id,
    string Name,
    string? ProfilePictureUrl);
