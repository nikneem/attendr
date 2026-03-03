using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.Presentations.CreatePresentation;

public sealed record CreatePresentationCommand(
    Guid ConferenceId,
    string Title,
    string Abstract,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    Guid RoomId,
    IReadOnlyList<Guid> SpeakerIds,
    Guid? RequestingProfileId,
    bool IsAdmin) : IAttendrCommand;
