using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.Speakers.CreateSpeaker;

public sealed record CreateSpeakerCommand(
    Guid ConferenceId,
    string Name,
    string? Company,
    string? ProfilePictureUrl,
    Guid? RequestingProfileId,
    bool IsAdmin) : IAttendrCommand;
