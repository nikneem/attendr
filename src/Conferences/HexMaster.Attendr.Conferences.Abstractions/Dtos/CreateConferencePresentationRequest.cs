namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

public sealed record CreateConferencePresentationRequest(
    string Title,
    string Abstract,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    Guid RoomId,
    List<Guid> SpeakerIds);
