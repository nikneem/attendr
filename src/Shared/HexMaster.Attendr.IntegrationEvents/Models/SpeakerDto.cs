namespace HexMaster.Attendr.IntegrationEvents.Models;

/// <summary>
/// DTO representing a speaker within an integration event.
/// </summary>
/// <param name="Id">The unique identifier of the speaker.</param>
/// <param name="Name">The display name of the speaker.</param>
/// <param name="ProfilePictureUrl">The optional URL of the speaker's profile picture.</param>
public sealed record SpeakerDto(Guid Id, string Name, string? ProfilePictureUrl);
