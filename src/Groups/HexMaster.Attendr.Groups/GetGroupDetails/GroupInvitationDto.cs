namespace HexMaster.Attendr.Groups.GetGroupDetails;

/// <summary>
/// DTO representing a pending invitation to a group.
/// </summary>
/// <param name="Id">The unique identifier of the invitee.</param>
/// <param name="Name">The name of the invitee.</param>
/// <param name="ExpirationDate">The date and time when the invitation expires.</param>
public sealed record GroupInvitationDto(
    Guid Id,
    string Name,
    DateTimeOffset ExpirationDate);
