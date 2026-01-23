using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Conferences.DomainModels;

/// <summary>
/// Represents a presentation at a conference.
/// </summary>
public sealed class Presentation : StatefulDomainModel<Guid>
{
    /// <summary>
    /// Gets the title of the presentation.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the abstract of the presentation.
    /// </summary>
    public string Abstract { get; private set; }

    /// <summary>
    /// Gets the start date and time of the presentation.
    /// </summary>
    public DateTime StartDateTime { get; private set; }

    /// <summary>
    /// Gets the end date and time of the presentation.
    /// </summary>
    public DateTime EndDateTime { get; private set; }

    /// <summary>
    /// Gets the room where the presentation is held.
    /// </summary>
    public Room Room { get; private set; }

    /// <summary>
    /// Gets the external ID from the synchronization source.
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this presentation has been analysed.
    /// </summary>
    public bool IsAnalysed { get; private set; }

    private readonly List<Speaker> _speakers = new();

    /// <summary>
    /// Gets the collection of speakers for this presentation.
    /// </summary>
    public IReadOnlyCollection<Speaker> Speakers => _speakers.AsReadOnly();

    private readonly List<PresentationTopic> _topics = new();

    /// <summary>
    /// Gets the collection of topics associated with this presentation.
    /// </summary>
    public IReadOnlyCollection<PresentationTopic> Topics => _topics.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Presentation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the presentation.</param>
    /// <param name="title">The title of the presentation.</param>
    /// <param name="abstract">The abstract of the presentation.</param>
    /// <param name="startDateTime">The start date and time.</param>
    /// <param name="endDateTime">The end date and time.</param>
    /// <param name="room">The room where the presentation is held.</param>
    /// <param name="speakers">The collection of speakers.</param>
    /// <param name="externalId">The external ID from the synchronization source.</param>
    /// <param name="isAnalysed">Whether the presentation has been analysed.</param>
    /// <param name="topics">The collection of topics associated with the presentation.</param>
    /// <param name="initialState">The initial state of the presentation.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    private Presentation(
        Guid id,
        string title,
        string @abstract,
        DateTime startDateTime,
        DateTime endDateTime,
        Room room,
        IEnumerable<Speaker> speakers,
        string? externalId,
        IEnumerable<PresentationTopic>? topics = null,
        bool isAnalysed = false,
        DomainModelState initialState = DomainModelState.Pristine)
        : base(id, initialState)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Presentation ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Presentation title cannot be empty.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(@abstract))
        {
            throw new ArgumentException("Presentation abstract cannot be empty.", nameof(@abstract));
        }

        if (endDateTime <= startDateTime)
        {
            throw new ArgumentException("End date/time must be after start date/time.", nameof(endDateTime));
        }

        ArgumentNullException.ThrowIfNull(room);

        ArgumentNullException.ThrowIfNull(speakers);

        var speakerList = speakers.ToList();
        if (speakerList.Count == 0)
        {
            throw new ArgumentException("Presentation must have at least one speaker.", nameof(speakers));
        }

        if (speakerList.Any(s => s == null))
        {
            throw new ArgumentException("Speakers cannot be null.", nameof(speakers));
        }

        Title = title;
        Abstract = @abstract;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        Room = room;
        ExternalId = externalId;
        IsAnalysed = isAnalysed;
        _speakers.AddRange(speakerList);

        if (topics != null)
        {
            _topics.AddRange(topics);
        }
    }

    /// <summary>
    /// Factory method to create a new presentation.
    /// </summary>
    /// <param name="title">The title of the presentation.</param>
    /// <param name="abstract">The abstract of the presentation.</param>
    /// <param name="startDateTime">The start date and time.</param>
    /// <param name="endDateTime">The end date and time.</param>
    /// <param name="room">The room where the presentation is held.</param>
    /// <param name="speakers">The collection of speakers.</param>
    /// <param name="externalId">The external ID from the synchronization source.</param>
    /// <param name="topics">The collection of topics associated with the presentation.</param>
    /// <returns>A new instance of <see cref="Presentation"/>.</returns>
    public static Presentation Create(
        string title,
        string @abstract,
        DateTime startDateTime,
        DateTime endDateTime,
        Room room,
        IEnumerable<Speaker> speakers,
        string? externalId = null,
        IEnumerable<PresentationTopic>? topics = null)
    {
        var id = Guid.NewGuid();
        return new Presentation(id, title, @abstract, startDateTime, endDateTime, room, speakers, externalId, topics, isAnalysed: false, DomainModelState.Created);
    }

    /// <summary>
    /// Factory method to load a presentation from persisted data.
    /// </summary>
    /// <param name="id">The unique identifier of the presentation.</param>
    /// <param name="title">The title of the presentation.</param>
    /// <param name="abstract">The abstract of the presentation.</param>
    /// <param name="startDateTime">The start date and time.</param>
    /// <param name="endDateTime">The end date and time.</param>
    /// <param name="room">The room where the presentation is held.</param>
    /// <param name="speakers">The collection of speakers.</param>
    /// <param name="externalId">The external ID from the synchronization source.</param>
    /// <param name="topics">The collection of topics associated with the presentation.</param>
    /// <param name="isAnalysed">Whether the presentation has been analysed.</param>
    /// <returns>A presentation instance loaded from persistence.</returns>
    public static Presentation FromPersisted(
        Guid id,
        string title,
        string @abstract,
        DateTime startDateTime,
        DateTime endDateTime,
        Room room,
        IEnumerable<Speaker> speakers,
        string? externalId = null,
        IEnumerable<PresentationTopic>? topics = null,
        bool isAnalysed = false)
    {
        return new Presentation(id, title, @abstract, startDateTime, endDateTime, room, speakers, externalId, topics, isAnalysed);
    }

    /// <summary>
    /// Updates the presentation details.
    /// </summary>
    /// <param name="title">The new title.</param>
    /// <param name="abstract">The new abstract.</param>
    /// <param name="startDateTime">The new start date and time.</param>
    /// <param name="endDateTime">The new end date and time.</param>
    public void UpdateDetails(string title, string @abstract, DateTime startDateTime, DateTime endDateTime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(@abstract, nameof(@abstract));

        if (endDateTime <= startDateTime)
        {
            throw new ArgumentException("End date/time must be after start date/time.", nameof(endDateTime));
        }

        if (ShouldUpdateProperty(Title, title))
        {
            Title = title;
            UpdateModifiedOn();
        }

        if (ShouldUpdateProperty(Abstract, @abstract))
        {
            Abstract = @abstract;
            IsAnalysed = false;
            UpdateModifiedOn();
        }

        if (ShouldUpdateProperty(StartDateTime, startDateTime))
        {
            StartDateTime = startDateTime;
            UpdateModifiedOn();
        }

        if (ShouldUpdateProperty(EndDateTime, endDateTime))
        {
            EndDateTime = endDateTime;
            UpdateModifiedOn();
        }
    }

    /// <summary>
    /// Changes the room for the presentation.
    /// </summary>
    /// <param name="room">The new room.</param>
    public void ChangeRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        if (Room.Id != room.Id)
        {
            Room = room;
            UpdateModifiedOn();
        }
    }

    /// <summary>
    /// Adds a speaker to the presentation.
    /// </summary>
    /// <param name="speaker">The speaker to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when speaker is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when speaker is already assigned.</exception>
    public void AddSpeaker(Speaker speaker)
    {
        ArgumentNullException.ThrowIfNull(speaker);

        if (_speakers.Any(s => s.Id == speaker.Id))
        {
            throw new InvalidOperationException($"Speaker with ID {speaker.Id} is already assigned to this presentation.");
        }

        TrackPropertyChange();
        _speakers.Add(speaker);
        SetState(DomainModelState.Modified);
        UpdateModifiedOn();
    }

    /// <summary>
    /// Removes a speaker from the presentation.
    /// </summary>
    /// <param name="speakerId">The ID of the speaker to remove.</param>
    /// <exception cref="ArgumentException">Thrown when speaker ID is empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when removing the last speaker or speaker not found.</exception>
    public void RemoveSpeaker(Guid speakerId)
    {
        if (speakerId == Guid.Empty)
        {
            throw new ArgumentException("Speaker ID cannot be empty.", nameof(speakerId));
        }

        if (_speakers.Count == 1)
        {
            throw new InvalidOperationException("Cannot remove the last speaker from a presentation.");
        }

        var speaker = _speakers.FirstOrDefault(s => s.Id == speakerId);
        if (speaker != null)
        {
            _speakers.Remove(speaker);
            SetState(DomainModelState.Modified);
            UpdateModifiedOn();
        }
        else
        {
            throw new InvalidOperationException($"Speaker with ID {speakerId} is not assigned to this presentation.");
        }
    }

    /// <summary>
    /// Adds a topic to the presentation.
    /// </summary>
    /// <param name="topic">The topic to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when topic is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when topic is already assigned.</exception>
    public void AddTopic(PresentationTopic topic)
    {
        ArgumentNullException.ThrowIfNull(topic);

        if (_topics.Any(t => t.Key.Equals(topic.Key, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Topic with key '{topic.Key}' is already assigned to this presentation.");
        }

        TrackPropertyChange();
        _topics.Add(topic);
        SetState(DomainModelState.Modified);
        UpdateModifiedOn();
    }

    /// <summary>
    /// Removes a topic from the presentation.
    /// </summary>
    /// <param name="topicKey">The key of the topic to remove.</param>
    /// <exception cref="ArgumentException">Thrown when topic key is empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when topic is not found.</exception>
    public void RemoveTopic(string topicKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicKey, nameof(topicKey));

        var topic = _topics.FirstOrDefault(t => t.Key.Equals(topicKey, StringComparison.OrdinalIgnoreCase));
        if (topic == null)
        {
            throw new InvalidOperationException($"Topic with key '{topicKey}' is not assigned to this presentation.");
        }

        _topics.Remove(topic);
        SetState(DomainModelState.Modified);
        UpdateModifiedOn();
    }

    /// <summary>
    /// Updates the topics for this presentation, replacing the existing set.
    /// </summary>
    /// <param name="topics">The new set of topics.</param>
    public void UpdateTopics(IEnumerable<PresentationTopic> topics)
    {
        ArgumentNullException.ThrowIfNull(topics);

        var newTopics = topics.ToList();

        // Check if topics have actually changed
        if (_topics.Count == newTopics.Count &&
            _topics.All(existing => newTopics.Any(newTopic =>
                newTopic.Key.Equals(existing.Key, StringComparison.OrdinalIgnoreCase) &&
                newTopic.Name.Equals(existing.Name, StringComparison.Ordinal))))
        {
            return; // No changes
        }

        TrackPropertyChange();
        _topics.Clear();
        _topics.AddRange(newTopics);
        SetState(DomainModelState.Modified);
        UpdateModifiedOn();
    }
}
