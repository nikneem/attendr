namespace HexMaster.Attendr.Profiles.Abstractions.Dtos;

/// <summary>
/// DTO containing detailed profile information.
/// </summary>
/// <param name="ProfileId">The unique identifier of the profile.</param>
/// <param name="DisplayName">The display name of the profile owner.</param>
/// <param name="FirstName">The first name of the profile owner.</param>
/// <param name="LastName">The last name of the profile owner.</param>
/// <param name="Email">The email address of the profile owner.</param>
/// <param name="ProfilePictureUrl">Optional URL to the profile picture.</param>
/// <param name="TagLine">Optional tagline or personal headline.</param>
/// <param name="IsSearchable">Whether the profile is searchable by other attendees.</param>
public sealed record ProfileDetailsDto(
    string ProfileId,
    string DisplayName,
    string? FirstName,
    string? LastName,
    string Email,
    string? ProfilePictureUrl,
    string? TagLine,
    bool IsSearchable);
