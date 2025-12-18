namespace HexMaster.Attendr.Profiles.Abstractions.Dtos;

/// <summary>
/// Result DTO for creating a user profile.
/// </summary>
/// <param name="ProfileId">The ID of the created profile.</param>
public sealed record CreateProfileResult(string ProfileId);
