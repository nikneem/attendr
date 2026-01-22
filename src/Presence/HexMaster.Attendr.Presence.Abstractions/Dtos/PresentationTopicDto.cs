namespace HexMaster.Attendr.Presence.Abstractions.Dtos;

/// <summary>
/// DTO representing a presentation topic with its key and name.
/// </summary>
/// <param name="Key">The unique key identifier for the topic.</param>
/// <param name="Name">The display name of the topic.</param>
public record PresentationTopicDto(string Key, string Name);
