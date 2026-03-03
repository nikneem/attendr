using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.ManagePresentations;

public sealed record CreateConferencePresentationCommand(
    Guid ConferenceId,
    string Title,
    string Abstract,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    Guid RoomId,
    List<Guid> SpeakerIds) : IAttendrCommand;
