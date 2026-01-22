namespace HexMaster.Attendr.Profiles.Abstractions.Dtos;

public sealed record UpdateProfileRequest(
    string DisplayName,
    string FirstName,
    string LastName,
    string? TagLine,
    bool IsSearchable);
