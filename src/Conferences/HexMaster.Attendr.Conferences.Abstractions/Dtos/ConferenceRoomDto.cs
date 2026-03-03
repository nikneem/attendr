namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

public sealed record ConferenceRoomDto(
    Guid Id,
    string Name,
    int Capacity,
    string? ExternalId);
