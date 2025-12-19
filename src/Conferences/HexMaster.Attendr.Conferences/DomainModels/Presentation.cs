namespace HexMaster.Attendr.Conferences.DomainModels;

/// <summary>
/// Represents a presentation at a conference.
/// </summary>
public sealed class Presentation
{
    /// <summary>
    /// Gets the unique identifier of the presentation.
    /// </summary>
    public Guid Id { get; private set; }

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
    /// Gets the ID of the room where the presentation is held.
    /// </summary>
    public Guid RoomId { get; private set; }

    private readonly List<Guid> _speakerIds = new();

    /// <summary>
    /// Gets the collection of speaker IDs for this presentation.
    /// </summary>
    public IReadOnlyCollection<Guid> SpeakerIds => _speakerIds.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Presentation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the presentation.</param>
    /// <param name="title">The title of the presentation.</param>
    /// <param name="abstract">The abstract of the presentation.</param>
    /// <param name="startDateTime">The start date and time.</param>
    /// <param name="endDateTime">The end date and time.</param>
    /// <param name="roomId">The ID of the room.</param>
    /// <param name="speakerIds">The collection of speaker IDs.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public Presentation(
        Guid id,
        string title,
        string @abstract,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid roomId,
        IEnumerable<Guid> speakerIds)
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

        if (roomId == Guid.Empty)
        {
            throw new ArgumentException("Room ID cannot be empty.", nameof(roomId));
        }

        ArgumentNullException.ThrowIfNull(speakerIds);

        var speakerIdList = speakerIds.ToList();
        if (speakerIdList.Count == 0)
        {
            throw new ArgumentException("Presentation must have at least one speaker.", nameof(speakerIds));
        }

        if (speakerIdList.Any(s => s == Guid.Empty))
        {
            throw new ArgumentException("Speaker IDs cannot be empty.", nameof(speakerIds));
        }

        Id = id;
        Title = title;
        Abstract = @abstract;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        RoomId = roomId;
        _speakerIds.AddRange(speakerIdList);
    }

    /// <summary>
    /// Factory method to create a new presentation.
    /// </summary>
    /// <param name="title">The title of the presentation.</param>
    /// <param name="abstract">The abstract of the presentation.</param>
    /// <param name="startDateTime">The start date and time.</param>
    /// <param name="endDateTime">The end date and time.</param>
    /// <param name="roomId">The ID of the room.</param>
    /// <param name="speakerIds">The collection of speaker IDs.</param>
    /// <returns>A new instance of <see cref="Presentation"/>.</returns>
    public static Presentation Create(
        string title,
        string @abstract,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid roomId,
        IEnumerable<Guid> speakerIds)
    {
        var id = Guid.NewGuid();
        return new Presentation(id, title, @abstract, startDateTime, endDateTime, roomId, speakerIds);
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

        Title = title;
        Abstract = @abstract;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
    }

    /// <summary>
    /// Changes the room for the presentation.
    /// </summary>
    /// <param name="roomId">The new room ID.</param>
    public void ChangeRoom(Guid roomId)
    {
        if (roomId == Guid.Empty)
        {
            throw new ArgumentException("Room ID cannot be empty.", nameof(roomId));
        }

        RoomId = roomId;
    }

    /// <summary>
    /// Adds a speaker to the presentation.
    /// </summary>
    /// <param name="speakerId">The ID of the speaker to add.</param>
    /// <exception cref="ArgumentException">Thrown when speaker ID is empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when speaker is already assigned.</exception>
    public void AddSpeaker(Guid speakerId)
    {
        if (speakerId == Guid.Empty)
        {
            throw new ArgumentException("Speaker ID cannot be empty.", nameof(speakerId));
        }

        if (_speakerIds.Contains(speakerId))
        {
            throw new InvalidOperationException($"Speaker with ID {speakerId} is already assigned to this presentation.");
        }

        _speakerIds.Add(speakerId);
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

        if (_speakerIds.Count == 1)
        {
            throw new InvalidOperationException("Cannot remove the last speaker from a presentation.");
        }

        if (!_speakerIds.Remove(speakerId))
        {
            throw new InvalidOperationException($"Speaker with ID {speakerId} is not assigned to this presentation.");
        }
    }
}
