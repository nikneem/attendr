namespace HexMaster.Attendr.Presence.Abstractions.Dtos;

/// <summary>
/// DTO for updating conference attendance status.
/// </summary>
/// <param name="IsAttending">Whether the user is attending the conference.</param>
public record UpdateAttendanceDto(bool IsAttending);
