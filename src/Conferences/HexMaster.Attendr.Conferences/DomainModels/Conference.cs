using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Conferences.DomainModels;

/// <summary>
/// Aggregate root representing a conference in the Attendr system.
/// Follows Domain-Driven Design principles with private constructor,
/// encapsulated collections, and behavior-focused methods.
/// </summary>
public sealed class Conference : StatefulDomainModel<Guid>
{
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
    /// Gets a value indicating whether the conference is visible to users.
    /// Defaults to false when created.
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// Gets the profile ID of the user who created this conference.
    /// Set once at creation time and immutable. Null for conferences created before ownership tracking.
    /// </summary>
    public Guid? CreatedByProfileId { get; private set; }

    /// <summary>
    /// Gets the synchronization source configuration for the conference.
    /// </summary>
    public SynchronizationSource? SynchronizationSource { get; private set; }

    public bool RoomsNeedSync { get; private set; }
    public bool SpeakersNeedSync { get; private set; }
    public bool PresentationsNeedSync { get; private set; }

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

    private Conference(
        Guid id,
        string title,
        string city,
        string country,
        DateOnly startDate,
        DateOnly endDate,
        string? imageUrl,
        bool isVisible,
        Guid? createdByProfileId = null,
        DomainModelState initialState = DomainModelState.Pristine)
        : base(id, initialState)
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

        Title = title;
        City = city;
        Country = country;
        StartDate = startDate;
        EndDate = endDate;
        ImageUrl = imageUrl;
        IsVisible = isVisible;
        CreatedByProfileId = createdByProfileId;
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
    /// <param name="createdByProfileId">The profile ID of the user creating the conference. Must not be empty when provided.</param>
    /// <returns>A new instance of <see cref="Conference"/>.</returns>
    public static Conference Create(
        string title,
        string city,
        string country,
        DateOnly startDate,
        DateOnly endDate,
        string? imageUrl = null,
        SynchronizationSource? synchronizationSource = null,
        Guid? createdByProfileId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(city, nameof(city));
        ArgumentException.ThrowIfNullOrWhiteSpace(country, nameof(country));

        if (createdByProfileId.HasValue && createdByProfileId.Value == Guid.Empty)
        {
            throw new ArgumentException("CreatedByProfileId cannot be an empty GUID when provided.", nameof(createdByProfileId));
        }

        var id = Guid.NewGuid();
        var conference = new Conference(id, title, city, country, startDate, endDate, imageUrl, false, createdByProfileId, DomainModelState.Created);
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
    /// <param name="isVisible">Whether the conference is visible to users.</param>
    /// <param name="synchronizationSource">Optional synchronization source configuration.</param>
    /// <param name="createdByProfileId">The profile ID of the user who created this conference.</param>
    /// <returns>A new instance of <see cref="Conference"/>.</returns>
    public static Conference FromPersisted(
        Guid id,
        string title,
        string city,
        string country,
        DateOnly startDate,
        DateOnly endDate,
        string? imageUrl = null,
        bool isVisible = false,
        SynchronizationSource? synchronizationSource = null,
        Guid? createdByProfileId = null)
    {
        var conference = new Conference(id, title, city, country, startDate, endDate, imageUrl, isVisible, createdByProfileId);
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
        RoomsNeedSync = true;
    }

    public void RemoveRoom(Guid roomId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == roomId)
            ?? throw new InvalidOperationException($"Room with ID {roomId} does not exist in the conference.");
        _rooms.Remove(room);
        RoomsNeedSync = true;
        SetState(DomainModelState.Touched);
    }

    public void RemoveSpeaker(Guid speakerId)
    {
        var speaker = _speakers.FirstOrDefault(s => s.Id == speakerId)
            ?? throw new InvalidOperationException($"Speaker with ID {speakerId} does not exist in the conference.");
        _speakers.Remove(speaker);
        SpeakersNeedSync = true;
        SetState(DomainModelState.Touched);
    }

    public void RemovePresentation(Guid presentationId)
    {
        var presentation = _presentations.FirstOrDefault(p => p.Id == presentationId)
            ?? throw new InvalidOperationException($"Presentation with ID {presentationId} does not exist in the conference.");
        _presentations.Remove(presentation);
        PresentationsNeedSync = true;
        SetState(DomainModelState.Touched);
    }

    public void MarkInvisibleDueToManualChanges()
    {
        UpdateVisibility(false);
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
        SpeakersNeedSync = true;
        SetState(DomainModelState.Touched);
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
        if (!_rooms.Any(r => r.Id == presentation.Room.Id))
        {
            throw new InvalidOperationException($"Room with ID {presentation.Room.Id} does not exist in the conference.");
        }

        // Validate all speakers exist
        foreach (var speaker in presentation.Speakers)
        {
            if (!_speakers.Any(s => s.Id == speaker.Id))
            {
                throw new InvalidOperationException($"Speaker with ID {speaker.Id} does not exist in the conference.");
            }
        }

        // Validate presentation dates are within conference dates
        //var presentationStartDate = DateOnly.FromDateTime(presentation.StartDateTime);
        //var presentationEndDate = DateOnly.FromDateTime(presentation.EndDateTime);

        //if (presentationStartDate < StartDate || presentationEndDate > EndDate)
        //{
        //    throw new InvalidOperationException("Presentation dates must be within conference dates.");
        //}

        _presentations.Add(presentation);
        PresentationsNeedSync = true;
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

        if (ShouldUpdateProperty(Title, title))
        {
            Title = title;
            UpdateModifiedOn();
        }

        if (ShouldUpdateProperty(City, city))
        {
            City = city;
            UpdateModifiedOn();
        }

        if (ShouldUpdateProperty(Country, country))
        {
            Country = country;
            UpdateModifiedOn();
        }

        if (ShouldUpdateProperty(StartDate, startDate))
        {
            StartDate = startDate;
            UpdateModifiedOn();
        }

        if (ShouldUpdateProperty(EndDate, endDate))
        {
            EndDate = endDate;
            UpdateModifiedOn();
        }

        if (ShouldUpdateProperty(ImageUrl, imageUrl))
        {
            ImageUrl = imageUrl;
            UpdateModifiedOn();
        }
    }

    /// <summary>
    /// Configures the synchronization source for the conference.
    /// </summary>
    /// <param name="synchronizationSource">The synchronization source configuration.</param>
    public void SetConfigureSynchronizationSource(SynchronizationSource? synchronizationSource)
    {
        var currentKey = SynchronizationSource?.SourceLocationOrApiKey;
        var newKey = synchronizationSource?.SourceLocationOrApiKey;
        var currentType = SynchronizationSource?.SourceType;
        var newType = synchronizationSource?.SourceType;

        if (currentType != newType || currentKey != newKey)
        {
            SynchronizationSource = synchronizationSource;
            UpdateModifiedOn();
            SetState(DomainModelState.Modified);
        }
    }

    /// <summary>
    /// Updates the visibility of the conference.
    /// </summary>
    /// <param name="isVisible">Whether the conference should be visible to users.</param>
    public void UpdateVisibility(bool isVisible)
    {
        if (ShouldUpdateProperty(IsVisible, isVisible))
        {
            IsVisible = isVisible;
            UpdateModifiedOn();
        }
    }

    public void UpdateSpeaker(Speaker speaker)
    {
        if (speaker.State != DomainModelState.Pristine && speaker.State != DomainModelState.Touched)
        {
            SetState(DomainModelState.Touched);
        }
    }

    /// <summary>
    /// Updates an existing room in the conference, tracking state changes.
    /// </summary>
    /// <param name="room">The room with updated properties.</param>
    public void UpdateRoom(Room room)
    {
        if (room.State != DomainModelState.Pristine && room.State != DomainModelState.Touched)
        {
            SetState(DomainModelState.Touched);
        }
    }

    /// <summary>
    /// Updates an existing presentation in the conference, tracking state changes.
    /// </summary>
    /// <param name="presentation">The presentation with updated properties.</param>
    public void UpdatePresentation(Presentation presentation)
    {
        if (presentation.State != DomainModelState.Pristine && presentation.State != DomainModelState.Touched)
        {
            SetState(DomainModelState.Touched);
        }
    }

    /// <summary>
    /// Removes rooms that are not used by any presentation.
    /// </summary>
    /// <returns>The number of rooms removed.</returns>
    public int RemoveUnusedRooms()
    {
        var usedRoomIds = _presentations.Select(p => p.Room.Id).Distinct().ToHashSet();
        var initialCount = _rooms.Count;

        _rooms.RemoveAll(r => !usedRoomIds.Contains(r.Id));

        var removedCount = initialCount - _rooms.Count;
        if (removedCount > 0)
        {
            SetState(DomainModelState.Touched);
        }

        return removedCount;
    }

    /// <summary>
    /// Removes speakers that are not associated with any presentation.
    /// </summary>
    /// <returns>The number of speakers removed.</returns>
    public int RemoveUnusedSpeakers()
    {
        var usedSpeakerIds = _presentations
            .SelectMany(p => p.Speakers.Select(s => s.Id))
            .Distinct()
            .ToHashSet();

        var initialCount = _speakers.Count;

        _speakers.RemoveAll(s => !usedSpeakerIds.Contains(s.Id));

        var removedCount = initialCount - _speakers.Count;
        if (removedCount > 0)
        {
            SetState(DomainModelState.Touched);
        }

        return removedCount;
    }
}
