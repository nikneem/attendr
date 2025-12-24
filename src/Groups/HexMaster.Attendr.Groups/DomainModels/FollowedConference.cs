namespace HexMaster.Attendr.Groups.DomainModels;

/// <summary>
/// Value object representing a conference followed by a group.
/// </summary>
public sealed class FollowedConference
{
    /// <summary>
    /// Gets the unique identifier of the conference.
    /// </summary>
    public Guid ConferenceId { get; private set; }

    /// <summary>
    /// Gets the name of the conference.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the city where the conference is held.
    /// </summary>
    public string City { get; private set; }

    /// <summary>
    /// Gets the country where the conference is held.
    /// </summary>
    public string Country { get; private set; }

    /// <summary>
    /// Gets an optional visual (image) for the conference.
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Gets the number of speakers linked to the conference.
    /// </summary>
    public int SpeakersCount { get; private set; }

    /// <summary>
    /// Gets the number of sessions/presentations linked to the conference.
    /// </summary>
    public int SessionsCount { get; private set; }

    /// <summary>
    /// Gets the start date of the conference.
    /// </summary>
    public DateOnly StartDate { get; private set; }

    /// <summary>
    /// Gets the end date of the conference.
    /// </summary>
    public DateOnly EndDate { get; private set; }

    private FollowedConference()
    {
        // For ORM/deserialization
        ConferenceId = Guid.Empty;
        Name = string.Empty;
        City = string.Empty;
        Country = string.Empty;
    }

    public FollowedConference(
        Guid conferenceId,
        string name,
        string city,
        string country,
        string? imageUrl,
        int speakersCount,
        int sessionsCount,
        DateOnly startDate,
        DateOnly endDate)
    {
        if (conferenceId == Guid.Empty)
        {
            throw new ArgumentException("Conference ID cannot be empty.", nameof(conferenceId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Conference name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City cannot be empty.", nameof(city));
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            throw new ArgumentException("Country cannot be empty.", nameof(country));
        }

        if (startDate > endDate)
        {
            throw new ArgumentException("Start date cannot be after end date.");
        }

        ConferenceId = conferenceId;
        Name = name;
        City = city;
        Country = country;
        ImageUrl = imageUrl;
        SpeakersCount = speakersCount;
        SessionsCount = sessionsCount;
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Gets the formatted location (City, Country).
    /// </summary>
    public string GetLocation() => $"{City}, {Country}";

    /// <summary>
    /// Checks if the conference is current or in the future.
    /// </summary>
    /// <returns>True if the conference end date is today or in the future.</returns>
    public bool IsCurrentOrFuture() => EndDate >= DateOnly.FromDateTime(DateTime.UtcNow);
}
