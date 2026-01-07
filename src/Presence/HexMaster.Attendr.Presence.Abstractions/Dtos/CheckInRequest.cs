namespace HexMaster.Attendr.Presence.Abstractions.Dtos;

/// <summary>
/// Request to check in or out of a presentation.
/// </summary>
public sealed record CheckInRequest(bool IsCheckedIn);
