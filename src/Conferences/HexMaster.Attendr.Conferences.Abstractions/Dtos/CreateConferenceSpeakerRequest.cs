namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

public sealed record CreateConferenceSpeakerRequest(string Name, string? Company, string? ProfilePictureUrl);
