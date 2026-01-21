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
    /// Gets the ID of the room where the presentation is held.
    /// </summary>
    public Guid RoomId { get; private set; }

    /// <summary>
    /// Gets the external ID from the synchronization source.
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this presentation has been analysed.
    /// </summary>
    public bool IsAnalysed { get; private set; }

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
    /// <param name="externalId">The external ID from the synchronization source.</param>
    /// <param name="isAnalysed">Whether the presentation has been analysed.</param>
    /// <param name="initialState">The initial state of the presentation.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    private Presentation(
        Guid id,
        string title,
        string @abstract,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid roomId,
        IEnumerable<Guid> speakerIds,
        string? externalId,
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

        Title = title;
        Abstract = @abstract;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        RoomId = roomId;
        ExternalId = externalId;
        IsAnalysed = isAnalysed;
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
    /// <param name="externalId">The external ID from the synchronization source.</param>
    /// <returns>A new instance of <see cref="Presentation"/>.</returns>
    public static Presentation Create(
        string title,
        string @abstract,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid roomId,
        IEnumerable<Guid> speakerIds,
        string? externalId = null)
    {
        var id = Guid.NewGuid();
        return new Presentation(id, title, @abstract, startDateTime, endDateTime, roomId, speakerIds, externalId, isAnalysed: false, DomainModelState.Created);
    }

    /// <summary>
    /// Factory method to load a presentation from persisted data.
    /// </summary>
    /// <param name="id">The unique identifier of the presentation.</param>
    /// <param name="title">The title of the presentation.</param>
    /// <param name="abstract">The abstract of the presentation.</param>
    /// <param name="startDateTime">The start date and time.</param>
    /// <param name="endDateTime">The end date and time.</param>
    /// <param name="roomId">The ID of the room.</param>
    /// <param name="speakerIds">The collection of speaker IDs.</param>
    /// <param name="externalId">The external ID from the synchronization source.</param>
    /// <param name="isAnalysed">Whether the presentation has been analysed.</param>
    /// <returns>A presentation instance loaded from persistence.</returns>
    public static Presentation FromPersisted(
        Guid id,
        string title,
        string @abstract,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid roomId,
        IEnumerable<Guid> speakerIds,
        string? externalId = null,
        bool isAnalysed = false)
    {
        return new Presentation(id, title, @abstract, startDateTime, endDateTime, roomId, speakerIds, externalId, isAnalysed);
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
    /// <param name="roomId">The new room ID.</param>
    public void ChangeRoom(Guid roomId)
    {
        if (roomId == Guid.Empty)
        {
            throw new ArgumentException("Room ID cannot be empty.", nameof(roomId));
        }

        if (ShouldUpdateProperty(RoomId, roomId))
        {
            RoomId = roomId;
            UpdateModifiedOn();
        }
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

        TrackPropertyChange();
        _speakerIds.Add(speakerId);
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

        if (_speakerIds.Count == 1)
        {
            throw new InvalidOperationException("Cannot remove the last speaker from a presentation.");
        }

        if (_speakerIds.Remove(speakerId))
        {
            SetState(DomainModelState.Modified);
            UpdateModifiedOn();
        }
        else
        {
            throw new InvalidOperationException($"Speaker with ID {speakerId} is not assigned to this presentation.");
        }
    }
}
