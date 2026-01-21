namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

/// <summary>
/// DTO representing a topic.
/// </summary>
/// <param name="Id">The unique identifier of the topic.</param>
/// <param name="Key">The unique key of the topic.</param>
/// <param name="Name">The display name of the topic.</param>
/// <param name="IsVisible">Whether the topic is visible to users.</param>
public sealed record TopicDto(
    Guid Id,
    string Key,
    string Name,
    bool IsVisible);
