using HexMaster.Attendr.Conferences.Abstractions.Dtos;

namespace HexMaster.Attendr.Conferences.Features.ListTopics;

/// <summary>
/// Result DTO for list topics operation.
/// </summary>
/// <param name="Topics">The list of topics.</param>
/// <param name="TotalCount">The total count of topics.</param>
public sealed record ListTopicsResult(
    List<TopicDto> Topics,
    int TotalCount);
