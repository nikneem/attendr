namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

/// <summary>
/// DTO for synchronization source configuration.
/// </summary>
/// <param name="SourceType">The type of synchronization source (e.g., Sessionize).</param>
/// <param name="SourceUrl">The source URL for the synchronization endpoint.</param>
public sealed record SynchronizationSourceDto(
    string SourceType,
    string SourceUrl);
