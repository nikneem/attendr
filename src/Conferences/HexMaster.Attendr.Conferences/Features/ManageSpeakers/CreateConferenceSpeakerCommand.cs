using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.ManageSpeakers;

public sealed record CreateConferenceSpeakerCommand(Guid ConferenceId, string Name, string? Company, string? ProfilePictureUrl) : IAttendrCommand;
