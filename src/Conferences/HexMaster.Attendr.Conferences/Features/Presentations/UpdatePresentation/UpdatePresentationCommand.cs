using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.Presentations.UpdatePresentation;

public sealed record UpdatePresentationCommand(
    Guid ConferenceId,
    Guid PresentationId,
    string Title,
    string Abstract,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    Guid RoomId,
    IReadOnlyList<Guid> SpeakerIds,
    Guid? RequestingProfileId,
    bool IsAdmin) : IAttendrCommand;
