namespace HexMaster.Attendr.Groups.Abstractions.Dtos;

/// <summary>
/// Request DTO for updating group details.
/// </summary>
public sealed record UpdateGroupRequest(
    string Name,
    bool IsPublic,
    bool IsSearchable);
