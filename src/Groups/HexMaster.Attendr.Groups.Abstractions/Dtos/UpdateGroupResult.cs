namespace HexMaster.Attendr.Groups.Abstractions.Dtos;

/// <summary>
/// Result DTO for updating a group.
/// </summary>
public sealed record UpdateGroupResult(
    Guid Id,
    string Name,
    bool IsPublic,
    bool IsSearchable);
