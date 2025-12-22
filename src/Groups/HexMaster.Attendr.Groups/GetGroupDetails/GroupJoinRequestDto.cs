namespace HexMaster.Attendr.Groups.GetGroupDetails;

/// <summary>
/// DTO representing a join request to a group.
/// </summary>
/// <param name="Id">The unique identifier of the requester.</param>
/// <param name="Name">The name of the requester.</param>
/// <param name="RequestedAt">The date and time when the request was made.</param>
public sealed record GroupJoinRequestDto(
    Guid Id,
    string Name,
    DateTimeOffset RequestedAt);
