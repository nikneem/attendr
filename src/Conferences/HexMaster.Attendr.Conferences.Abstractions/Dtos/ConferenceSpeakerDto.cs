namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

public sealed record ConferenceSpeakerDto(
    Guid Id,
    string Name,
    string? Company,
    string? ProfilePictureUrl,
    string? ExternalId);
