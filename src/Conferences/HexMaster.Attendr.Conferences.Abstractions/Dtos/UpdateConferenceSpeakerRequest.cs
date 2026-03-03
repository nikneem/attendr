namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

public sealed record UpdateConferenceSpeakerRequest(string Name, string? Company, string? ProfilePictureUrl);
