using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.Presentations.DeletePresentation;

public sealed record DeletePresentationCommand(
    Guid ConferenceId,
    Guid PresentationId,
    Guid? RequestingProfileId,
    bool IsAdmin) : IAttendrCommand;
