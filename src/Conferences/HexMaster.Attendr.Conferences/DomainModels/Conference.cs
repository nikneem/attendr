namespace HexMaster.Attendr.Conferences.DomainModels;

/// <summary>
/// Aggregate root representing a conference in the Attendr system.
/// Follows Domain-Driven Design principles with private constructor,
/// encapsulated collections, and behavior-focused methods.
/// </summary>
public sealed class Conference
{
    /// <summary>
    /// Gets the unique identifier for the conference.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the title of the conference.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the city where the conference is held.
    /// </summary>
    public string City { get; private set; }

    /// <summary>
    /// Gets the country where the conference is held.
    /// </summary>
    public string Country { get; private set; }

    /// <summary>
    /// Gets the start date of the conference.
    /// </summary>
    public DateOnly StartDate { get; private set; }

    /// <summary>
    /// Gets the end date of the conference.
    /// </summary>
    public DateOnly EndDate { get; private set; }

    /// <summary>
    /// Gets the URL to an image representing the conference (logo or visual).
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Gets the synchronization source configuration for the conference.
    /// </summary>
    public SynchronizationSource? SynchronizationSource { get; private set; }

    private readonly List<Room> _rooms = new();
    private readonly List<Speaker> _speakers = new();
    private readonly List<Presentation> _presentations = new();

    /// <summary>
    /// Gets the collection of rooms at the conference.
    /// </summary>
    public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();

    /// <summary>
    /// Gets the collection of speakers at the conference.
    /// </summary>
    public IReadOnlyCollection<Speaker> Speakers => _speakers.AsReadOnly();

    /// <summary>
    /// Gets the collection of presentations at the conference.
    /// </summary>
    public IReadOnlyCollection<Presentation> Presentations => _presentations.AsReadOnly();

    private Conference()
    {
        // For ORM/deserialization
        Id = Guid.Empty;
        Title = string.Empty;
        City = string.Empty;
        Country = string.Empty;
        StartDate = DateOnly.MinValue;
        EndDate = DateOnly.MinValue;
    }

    private Conference(
        Guid id,
        string title,
        string city,
        string country,
        DateOnly startDate,
        DateOnly endDate,
        string? imageUrl = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Conference ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Conference title cannot be empty.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("Conference city cannot be empty.", nameof(city));
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            throw new ArgumentException("Conference country cannot be empty.", nameof(country));
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        }

        Id = id;
        Title = title;
        City = city;
        Country = country;
        StartDate = startDate;
        EndDate = endDate;
        ImageUrl = imageUrl;
    }

    /// <summary>
    /// Factory method to create a new conference.
    /// </summary>
    /// <param name="title">The title of the conference.</param>
    /// <param name="city">The city where the conference is held.</param>
    /// <param name="country">The country where the conference is held.</param>
    /// <param name="startDate">The start date of the conference.</param>
    /// <param name="endDate">The end date of the conference.</param>
    /// <param name="imageUrl">Optional URL to an image representing the conference.</param>
    /// <param name="synchronizationSource">Optional synchronization source configuration.</param>
    /// <returns>A new instance of <see cref="Conference"/>.</returns>
    public static Conference Create(
        string title,
        string city,
        string country,
        DateOnly startDate,
        DateOnly endDate,
        string? imageUrl = null,
        SynchronizationSource? synchronizationSource = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(city, nameof(city));
        ArgumentException.ThrowIfNullOrWhiteSpace(country, nameof(country));

        var id = Guid.NewGuid();
        var conference = new Conference(id, title, city, country, startDate, endDate, imageUrl);
        conference.SynchronizationSource = synchronizationSource;
        return conference;
    }

    /// <summary>
    /// Factory method to create a conference from persisted data.
    /// </summary>
    /// <param name="id">The ID of the conference.</param>
    /// <param name="title">The title of the conference.</param>
    /// <param name="city">The city where the conference is held.</param>
    /// <param name="country">The country where the conference is held.</param>
    /// <param name="startDate">The start date of the conference.</param>
    /// <param name="endDate">The end date of the conference.</param>
    /// <param name="imageUrl">Optional URL to an image representing the conference.</param>
    /// <param name="synchronizationSource">Optional synchronization source configuration.</param>
    /// <returns>A new instance of <see cref="Conference"/>.</returns>
    public static Conference FromPersisted(
        Guid id,
        string title,
        string city,
        string country,
        DateOnly startDate,
        DateOnly endDate,
        string? imageUrl = null,
        SynchronizationSource? synchronizationSource = null)
    {
        var conference = new Conference(id, title, city, country, startDate, endDate, imageUrl);
        conference.SynchronizationSource = synchronizationSource;
        return conference;
    }

    /// <summary>
    /// Adds a room to the conference.
    /// </summary>
    /// <param name="room">The room to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when room is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a room with the same ID already exists.</exception>
    public void AddRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        if (_rooms.Any(r => r.Id == room.Id))
        {
            throw new InvalidOperationException($"Room with ID {room.Id} already exists in the conference.");
        }

        _rooms.Add(room);
    }

    /// <summary>
    /// Adds a speaker to the conference.
    /// </summary>
    /// <param name="speaker">The speaker to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when speaker is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a speaker with the same ID already exists.</exception>
    public void AddSpeaker(Speaker speaker)
    {
        ArgumentNullException.ThrowIfNull(speaker);

        if (_speakers.Any(s => s.Id == speaker.Id))
        {
            throw new InvalidOperationException($"Speaker with ID {speaker.Id} already exists in the conference.");
        }

        _speakers.Add(speaker);
    }

    /// <summary>
    /// Adds a presentation to the conference.
    /// </summary>
    /// <param name="presentation">The presentation to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when presentation is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public void AddPresentation(Presentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        if (_presentations.Any(p => p.Id == presentation.Id))
        {
            throw new InvalidOperationException($"Presentation with ID {presentation.Id} already exists in the conference.");
        }

        // Validate room exists
        if (!_rooms.Any(r => r.Id == presentation.RoomId))
        {
            throw new InvalidOperationException($"Room with ID {presentation.RoomId} does not exist in the conference.");
        }

        // Validate all speakers exist
        foreach (var speakerId in presentation.SpeakerIds)
        {
            if (!_speakers.Any(s => s.Id == speakerId))
            {
                throw new InvalidOperationException($"Speaker with ID {speakerId} does not exist in the conference.");
            }
        }

        // Validate presentation dates are within conference dates
        var presentationStartDate = DateOnly.FromDateTime(presentation.StartDateTime);
        var presentationEndDate = DateOnly.FromDateTime(presentation.EndDateTime);

        if (presentationStartDate < StartDate || presentationEndDate > EndDate)
        {
            throw new InvalidOperationException("Presentation dates must be within conference dates.");
        }

        _presentations.Add(presentation);
    }

    /// <summary>
    /// Updates the conference details.
    /// </summary>
    /// <param name="title">The new title.</param>
    /// <param name="city">The new city.</param>
    /// <param name="country">The new country.</param>
    /// <param name="startDate">The new start date.</param>
    /// <param name="endDate">The new end date.</param>
    /// <param name="imageUrl">Optional URL to an image representing the conference.</param>
    public void UpdateDetails(string title, string city, string country, DateOnly startDate, DateOnly endDate, string? imageUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(city, nameof(city));
        ArgumentException.ThrowIfNullOrWhiteSpace(country, nameof(country));

        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        }

        Title = title;
        City = city;
        Country = country;
        StartDate = startDate;
        EndDate = endDate;
        ImageUrl = imageUrl;
    }

    /// <summary>
    /// Configures the synchronization source for the conference.
    /// </summary>
    /// <param name="synchronizationSource">The synchronization source configuration.</param>
    public void ConfigureSynchronizationSource(SynchronizationSource? synchronizationSource)
    {
        SynchronizationSource = synchronizationSource;
    }
}
