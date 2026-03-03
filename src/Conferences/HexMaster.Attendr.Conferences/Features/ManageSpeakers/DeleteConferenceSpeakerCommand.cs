using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.ManageSpeakers;

public sealed record DeleteConferenceSpeakerCommand(Guid ConferenceId, Guid SpeakerId) : IAttendrCommand;
