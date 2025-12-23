namespace HexMaster.Attendr.Conferences.DomainModels;

/// <summary>
/// Represents a speaker at a conference.
/// </summary>
public sealed class Speaker
{
    /// <summary>
    /// Gets the unique identifier of the speaker.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the name of the speaker.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the external ID from the synchronization source.
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>
    /// Gets the company or organization the speaker represents.
    /// </summary>
    public string? Company { get; private set; }

    /// <summary>
    /// Gets the URL to the speaker's profile picture.
    /// </summary>
    public string? ProfilePictureUrl { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Speaker"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the speaker.</param>
    /// <param name="name">The name of the speaker.</param>
    /// <param name="company">The company or organization the speaker represents.</param>
    /// <param name="profilePictureUrl">The URL to the speaker's profile picture.</param>
    /// <param name="externalId">The external ID from the synchronization source.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public Speaker(Guid id, string name, string? company = null, string? profilePictureUrl = null, string? externalId = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Speaker ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Speaker name cannot be empty.", nameof(name));
        }

        Id = id;
        Name = name;
        Company = company;
        ProfilePictureUrl = profilePictureUrl;
        ExternalId = externalId;
    }

    /// <summary>
    /// Factory method to create a new speaker.
    /// </summary>
    /// <param name="name">The name of the speaker.</param>
    /// <param name="company">The company or organization the speaker represents.</param>
    /// <param name="profilePictureUrl">The URL to the speaker's profile picture.</param>
    /// <param name="externalId">The external ID from the synchronization source.</param>
    /// <returns>A new instance of <see cref="Speaker"/>.</returns>
    public static Speaker Create(string name, string? company = null, string? profilePictureUrl = null, string? externalId = null)
    {
        var id = Guid.NewGuid();
        return new Speaker(id, name, company, profilePictureUrl, externalId);
    }
}
