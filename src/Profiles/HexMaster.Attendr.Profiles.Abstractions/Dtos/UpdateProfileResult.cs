namespace HexMaster.Attendr.Profiles.Abstractions.Dtos;

public sealed record UpdateProfileResult(
    string ProfileId,
    string DisplayName,
    string FirstName,
    string LastName,
    string? TagLine,
    bool IsSearchable);
