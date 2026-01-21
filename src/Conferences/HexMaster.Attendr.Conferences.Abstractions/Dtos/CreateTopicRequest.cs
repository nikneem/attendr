namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

/// <summary>
/// Request DTO for creating a new topic.
/// </summary>
/// <param name="Key">The unique key of the topic.</param>
/// <param name="Name">The display name of the topic.</param>
public sealed record CreateTopicRequest(
    string Key,
    string Name);
