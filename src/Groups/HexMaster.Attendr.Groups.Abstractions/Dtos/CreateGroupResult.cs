using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Abstractions.Dtos;

/// <summary>
/// Result DTO for group creation.
/// </summary>
public sealed record CreateGroupResult(
    Guid Id,
    string Name,
    IReadOnlyCollection<GroupMemberDto> Members);

public sealed record GroupMemberDto(
    Guid Id,
    string Name,
    GroupRole Role);
