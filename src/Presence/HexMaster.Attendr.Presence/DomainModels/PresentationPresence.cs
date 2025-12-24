namespace HexMaster.Attendr.Presence.DomainModels;

public sealed class PresentationPresence
{
    public Guid PresentationId { get; private set; }
    public string Title { get; private set; }
    public string Abstract { get; private set; }
    public string Room { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public bool IsRated { get; private set; }
    public bool IsFavorite { get; private set; }
    public bool IsCheckedIn { get; private set; }
    public byte? Rating { get; private set; }

    private readonly List<PresentationSpeaker> _speakers = new();
    public IReadOnlyCollection<PresentationSpeaker> Speakers => _speakers.AsReadOnly();

    private PresentationPresence()
    {
        PresentationId = Guid.Empty;
        Title = string.Empty;
        Abstract = string.Empty;
        Room = string.Empty;
        StartDateTime = DateTime.MinValue;
        EndDateTime = DateTime.MinValue;
    }

    public PresentationPresence(
        Guid presentationId,
        string title,
        string @abstract,
        string room,
        DateTime startDateTime,
        DateTime endDateTime,
        IEnumerable<PresentationSpeaker>? speakers = null,
        bool isRated = false,
        bool isFavorite = false,
        bool isCheckedIn = false,
        byte? rating = null)
    {
        if (presentationId == Guid.Empty)
        {
            throw new ArgumentException("Presentation ID cannot be empty.", nameof(presentationId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(@abstract, nameof(@abstract));
        ArgumentException.ThrowIfNullOrWhiteSpace(room, nameof(room));

        if (endDateTime <= startDateTime)
        {
            throw new ArgumentException("End date/time must be after start date/time.", nameof(endDateTime));
        }

        PresentationId = presentationId;
        Title = title;
        Abstract = @abstract;
        Room = room;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        IsRated = isRated;
        IsFavorite = isFavorite;
        IsCheckedIn = isCheckedIn;
        Rating = rating;

        if (speakers != null)
        {
            _speakers.AddRange(speakers);
        }
    }

    public void AddSpeaker(PresentationSpeaker speaker)
    {
        ArgumentNullException.ThrowIfNull(speaker);

        if (_speakers.Any(s => s.SpeakerId == speaker.SpeakerId))
        {
            throw new InvalidOperationException($"Speaker with ID {speaker.SpeakerId} already exists for this presentation.");
        }

        _speakers.Add(speaker);
    }
}
