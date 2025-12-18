namespace HexMaster.Attendr.Profiles.CreateProfile;

/// <summary>
/// Command to create a new user profile.
/// </summary>
/// <param name="SubjectId">The subject ID from the authentication provider (e.g., Auth0).</param>
/// <param name="DisplayName">The display name for the profile.</param>
/// <param name="FirstName">The first name of the profile owner.</param>
/// <param name="LastName">The last name of the profile owner.</param>
/// <param name="Email">The email address of the profile owner.</param>
public sealed record CreateProfileCommand(
    string SubjectId,
    string DisplayName,
    string FirstName,
    string LastName,
    string Email);
