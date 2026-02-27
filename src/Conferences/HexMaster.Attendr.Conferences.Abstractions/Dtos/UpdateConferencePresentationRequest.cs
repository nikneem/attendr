namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

public sealed record UpdateConferencePresentationRequest(
    string Title,
    string Abstract,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    Guid RoomId,
    IReadOnlyList<Guid> SpeakerIds);
