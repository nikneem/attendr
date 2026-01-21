namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

/// <summary>
/// Request DTO for updating a topic.
/// </summary>
/// <param name="Key">The unique key of the topic.</param>
/// <param name="Name">The display name of the topic.</param>
/// <param name="IsVisible">Whether the topic should be visible to users.</param>
public sealed record UpdateTopicRequest(
    string Key,
    string Name,
    bool IsVisible);
