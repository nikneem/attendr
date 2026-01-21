namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

/// <summary>
/// DTO for topic reference with minimal information.
/// </summary>
/// <param name="Key">The normalized key of the topic.</param>
/// <param name="Name">The display name of the topic.</param>
public sealed record TopicReferenceDto(
    string Key,
    string Name);
