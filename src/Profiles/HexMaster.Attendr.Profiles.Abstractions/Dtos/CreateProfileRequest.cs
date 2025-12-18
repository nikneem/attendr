namespace HexMaster.Attendr.Profiles.Abstractions.Dtos;

/// <summary>
/// Request DTO for creating a new user profile.
/// </summary>
public sealed record CreateProfileRequest
{
    /// <summary>
    /// Gets or sets the first name of the profile owner.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name of the profile owner.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the profile owner.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
