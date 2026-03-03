namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

public sealed record ConferencePresentationDto(
    Guid Id,
    string Title,
    string Abstract,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    Guid RoomId,
    string RoomName,
    List<Guid> SpeakerIds,
    List<ConferenceSpeakerDto> Speakers);
