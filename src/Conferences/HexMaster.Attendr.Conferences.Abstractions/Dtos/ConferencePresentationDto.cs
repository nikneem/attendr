namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

public sealed record ConferencePresentationDto(
    Guid Id,
    string Title,
    string Abstract,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    Guid RoomId,
    string RoomName,
    IReadOnlyList<Guid> SpeakerIds,
    IReadOnlyList<ConferenceSpeakerDto> Speakers,
    string? ExternalId);
