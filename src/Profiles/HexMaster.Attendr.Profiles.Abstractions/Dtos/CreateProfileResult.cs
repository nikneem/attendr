namespace HexMaster.Attendr.Profiles.Abstractions.Dtos;

/// <summary>
/// Result DTO for creating a user profile.
/// </summary>
/// <param name="ProfileId">The ID of the created profile.</param>
/// <param name="FirstName">The first name of the profile.</param>
/// <param name="LastName">The last name of the profile.</param>
/// <param name="Email">The email address of the profile.</param>
/// <param name="DisplayName">The display name of the profile.</param>
public sealed record CreateProfileResult(
    string ProfileId,
    string FirstName,
    string LastName,
    string Email,
    string DisplayName);
