using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.ManagePresentations;

public sealed record DeleteConferencePresentationCommand(Guid ConferenceId, Guid PresentationId) : IAttendrCommand;
