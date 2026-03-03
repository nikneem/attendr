using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.Speakers.UpdateSpeaker;

public sealed record UpdateSpeakerCommand(
    Guid ConferenceId,
    Guid SpeakerId,
    string Name,
    string? Company,
    string? ProfilePictureUrl,
    Guid? RequestingProfileId,
    bool IsAdmin) : IAttendrCommand;
