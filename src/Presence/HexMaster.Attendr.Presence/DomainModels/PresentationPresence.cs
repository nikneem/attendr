namespace HexMaster.Attendr.Presence.DomainModels;

public sealed class PresentationPresence
{
    public Guid ProfileId { get; private set; }
    public Guid ConferenceId { get; private set; }
    public Guid PresentationId { get; private set; }
    public string Title { get; private set; }
    public string Abstract { get; private set; }
    public string Room { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public bool IsRated { get; private set; }
    public bool IsFavorite { get; private set; }
    public bool IsCheckedIn { get; private set; }
    public DateTimeOffset? CheckedInAt { get; private set; }
    public byte? Rating { get; private set; }

    private readonly List<PresentationSpeaker> _speakers = new();
    public IReadOnlyCollection<PresentationSpeaker> Speakers => _speakers.AsReadOnly();

    public PresentationPresence(
        Guid profileId,
        Guid conferenceId,
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
        DateTimeOffset? checkedInAt = null,
        byte? rating = null)
    {
        if (profileId == Guid.Empty)
        {
            throw new ArgumentException("Profile ID cannot be empty.", nameof(profileId));
        }

        if (conferenceId == Guid.Empty)
        {
            throw new ArgumentException("Conference ID cannot be empty.", nameof(conferenceId));
        }

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

        ProfileId = profileId;
        ConferenceId = conferenceId;
        PresentationId = presentationId;
        Title = title;
        Abstract = @abstract;
        Room = room;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        IsRated = isRated;
        IsFavorite = isFavorite;
        IsCheckedIn = isCheckedIn;
        CheckedInAt = checkedInAt;
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

    public void UpdatePresentationInfo(
        string title,
        string @abstract,
        string room,
        DateTime startDateTime,
        DateTime endDateTime,
        IEnumerable<PresentationSpeaker> speakers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(@abstract, nameof(@abstract));
        ArgumentException.ThrowIfNullOrWhiteSpace(room, nameof(room));

        if (endDateTime <= startDateTime)
        {
            throw new ArgumentException("End date/time must be after start date/time.", nameof(endDateTime));
        }

        Title = title;
        Abstract = @abstract;
        Room = room;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;

        _speakers.Clear();
        if (speakers != null)
        {
            _speakers.AddRange(speakers);
        }
    }

    public void RatePresentation(byte? rating, bool isFavorite)
    {
        if (rating.HasValue && rating.Value > 5)
        {
            throw new ArgumentException("Rating must be between 0 and 5.", nameof(rating));
        }

        IsRated = true;
        Rating = rating;
        IsFavorite = isFavorite;
    }

    public void CheckIn()
    {
        IsCheckedIn = true;
        CheckedInAt = DateTimeOffset.UtcNow;
    }

    public void CheckOut()
    {
        IsCheckedIn = false;
        CheckedInAt = null;
    }
}
