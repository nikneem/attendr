using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.Speakers.DeleteSpeaker;

public sealed record DeleteSpeakerCommand(
    Guid ConferenceId,
    Guid SpeakerId,
    Guid? RequestingProfileId,
    bool IsAdmin) : IAttendrCommand;
