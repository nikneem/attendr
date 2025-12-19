using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Profiles.DomainModels;

/// <summary>
/// Represents an attendee profile in the Attendr system.
/// Contains user profile information including personal details and profile settings.
/// Follows Domain-Driven Design principles with private setters and behavior methods.
/// </summary>
public class Profile : DomainModel<string>
{
    /// <summary>
    /// Gets the subject ID from the authentication provider (e.g., Auth0).
    /// </summary>
    public string SubjectId { get; private set; }

    /// <summary>
    /// Gets the display name of the profile owner.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the first name of the profile owner.
    /// </summary>
    public string? FirstName { get; private set; }
    /// <summary>
    /// Gets the last name of the profile owner.
    /// </summary>
    public string? LastName { get; private set; }

    /// <summary>
    /// Gets the email address of the profile owner.
    /// </summary>
    public string Email { get; private set; }
    /// <summary>
    /// Gets the employee identifier or employee status.
    /// </summary>
    public string? Employee { get; private set; }

    /// <summary>
    /// Gets the tag line or personal headline of the profile owner.
    /// </summary>
    public string? TagLine { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the profile is searchable by other attendees.
    /// </summary>
    public bool IsSearchable { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the profile is enabled.
    /// </summary>
    public bool Enabled { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Profile"/> class.
    /// Private constructor for ORM/deserialization and internal use by factory methods.
    /// </summary>
    /// <param name="id">The immutable unique identifier for the profile.</param>
    /// <param name="subjectId">The subject ID from the authentication provider.</param>
    /// <param name="displayName">The display name of the profile owner.</param>
    /// <param name="firstName">The first name of the profile owner.</param>
    /// <param name="lastName">The last name of the profile owner.</param>
    /// <param name="email">The email address of the profile owner.</param>
    /// <exception cref="ArgumentNullException">Thrown when id or subjectId is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or whitespace.</exception>
    private Profile(string id,
        string subjectId,
         string displayName,
          string firstName,
           string lastName,
            string email,
            string? employee,
              string? tagLine,
             bool isEnabled,
              bool isSearchable)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectId, nameof(subjectId));
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));

        if (!email.Contains("@") || !email.Contains("."))
        {
            throw new ArgumentException("Email format is invalid.", nameof(email));
        }

        SubjectId = subjectId.Trim();
        DisplayName = displayName.Trim();
        FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Employee = string.IsNullOrWhiteSpace(employee) ? null : employee.Trim();
        TagLine = string.IsNullOrWhiteSpace(tagLine) ? null : tagLine.Trim();
        Enabled = isEnabled;
        IsSearchable = isSearchable;
    }

    /// <summary>
    /// Factory method to create a new profile.
    /// </summary>
    /// <param name="subjectId">The subject ID from the authentication provider.</param>
    /// <param name="displayName">The display name of the profile owner.</param>
    /// <param name="firstName">The first name of the profile owner.</param>
    /// <param name="lastName">The last name of the profile owner.</param>
    /// <param name="email">The email address of the profile owner.</param>
    /// <returns>A new instance of <see cref="Profile"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are invalid.</exception>
    public static Profile Create(
        string subjectId,
        string displayName,
        string firstName,
        string lastName,
        string email)
    {
        var profileId = Guid.NewGuid().ToString();
        return new Profile(
            profileId,
            subjectId,
            displayName,
            firstName,
            lastName,
            email,
            null,
            null,
            true,
            false);
    }

    /// <summary>
    /// Factory method to load a profile from persisted data.
    /// </summary>
    /// <param name="id">The profile ID.</param>
    /// <param name="subjectId">The subject ID from the authentication provider.</param>
    /// <param name="displayName">The display name of the profile owner.</param>
    /// <param name="firstName">The first name of the profile owner.</param>
    /// <param name="lastName">The last name of the profile owner.</param>
    /// <param name="email">The email address of the profile owner.</param>
    /// <param name="employee">The employee identifier.</param>
    /// <param name="tagLine">The profile tag line.</param>
    /// <param name="isEnabled">Whether the profile is enabled.</param>
    /// <param name="isSearchable">Whether the profile is searchable.</param>
    /// <returns>A profile instance loaded from persistence.</returns>
    public static Profile FromPersisted(
        string id,
        string subjectId,
        string displayName,
        string firstName,
        string lastName,
        string email,
        string? employee,
        string? tagLine,
        bool isEnabled,
        bool isSearchable)
    {
        return new Profile(
            id,
            subjectId,
            displayName,
            firstName,
            lastName,
            email,
            employee,
            tagLine,
            isEnabled,
            isSearchable);
    }

    /// <summary>
    /// Sets the subject ID from the authentication provider.
    /// </summary>
    /// <param name="subjectId">The subject ID to set.</param>
    /// <exception cref="ArgumentException">Thrown when subjectId is null or whitespace.</exception>
    public void SetSubjectId(string subjectId)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new ArgumentException("Subject ID cannot be null or whitespace.", nameof(subjectId));
        }

        SubjectId = subjectId.Trim();
        UpdateModifiedOn();
    }

    /// <summary>
    /// Sets the display name of the profile owner.
    /// </summary>
    /// <param name="displayName">The display name to set.</param>
    /// <exception cref="ArgumentException">Thrown when displayName is null or whitespace.</exception>
    public void SetDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be null or whitespace.", nameof(displayName));
        }

        DisplayName = displayName.Trim();
        UpdateModifiedOn();
    }

    /// <summary>
    /// Sets the first name of the profile owner with validation.
    /// </summary>
    /// <param name="firstName">The first name to set.</param>
    /// <exception cref="ArgumentException">Thrown when firstName is null or whitespace.</exception>
    public void SetFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be null or whitespace.", nameof(firstName));
        }

        FirstName = firstName.Trim();
        UpdateModifiedOn();
    }

    /// <summary>
    /// Sets the last name of the profile owner with validation.
    /// </summary>
    /// <param name="lastName">The last name to set.</param>
    /// <exception cref="ArgumentException">Thrown when lastName is null or whitespace.</exception>
    public void SetLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be null or whitespace.", nameof(lastName));
        }

        LastName = lastName.Trim();
        UpdateModifiedOn();
    }

    /// <summary>
    /// Sets the email address of the profile owner with validation.
    /// </summary>
    /// <param name="email">The email address to set.</param>
    /// <exception cref="ArgumentException">Thrown when email is invalid.</exception>
    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            throw new ArgumentException("Email format is invalid.", nameof(email));
        }

        Email = email.Trim().ToLowerInvariant();
        UpdateModifiedOn();
    }

    /// <summary>
    /// Sets the employee identifier or employee status with validation.
    /// </summary>
    /// <param name="employee">The employee identifier to set.</param>
    /// <exception cref="ArgumentException">Thrown when employee is null or whitespace.</exception>
    public void SetEmployee(string employee)
    {
        if (string.IsNullOrWhiteSpace(employee))
        {
            throw new ArgumentException("Employee cannot be null or whitespace.", nameof(employee));
        }

        Employee = employee.Trim();
        UpdateModifiedOn();
    }

    /// <summary>
    /// Sets the tag line or personal headline with validation.
    /// </summary>
    /// <param name="tagLine">The tag line to set. Can be empty or null to clear.</param>
    public void SetTagLine(string? tagLine)
    {
        TagLine = string.IsNullOrWhiteSpace(tagLine) ? string.Empty : tagLine.Trim();
        UpdateModifiedOn();
    }

    /// <summary>
    /// Sets whether the profile is searchable by other attendees.
    /// </summary>
    /// <param name="isSearchable">True if the profile should be searchable; otherwise, false.</param>
    public void SetIsSearchable(bool isSearchable)
    {
        IsSearchable = isSearchable;
        UpdateModifiedOn();
    }

    /// <summary>
    /// Sets whether the profile is enabled.
    /// </summary>
    /// <param name="enabled">True if the profile should be enabled; otherwise, false.</param>
    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
        UpdateModifiedOn();
    }

}
