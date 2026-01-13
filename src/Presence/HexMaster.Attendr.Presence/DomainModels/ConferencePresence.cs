namespace HexMaster.Attendr.Presence.DomainModels;

public sealed class ConferencePresence
{
    public Guid ConferenceId { get; private set; }
    public string ConferenceName { get; private set; }
    public string Location { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public Guid ProfileId { get; private set; }
    public bool IsFollowing { get; private set; }
    public bool IsAttending { get; private set; }

    private readonly List<PresentationPresence> _presentations = new();
    public IReadOnlyCollection<PresentationPresence> Presentations => _presentations.AsReadOnly();

    public ConferencePresence(
        Guid conferenceId,
        string conferenceName,
        string location,
        DateOnly startDate,
        DateOnly endDate,
        Guid profileId,
        string? imageUrl = null,
        bool isFollowing = false,
        bool isAttending = false,
        IEnumerable<PresentationPresence>? presentations = null)
    {
        if (conferenceId == Guid.Empty)
        {
            throw new ArgumentException("Conference ID cannot be empty.", nameof(conferenceId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(conferenceName, nameof(conferenceName));
        ArgumentException.ThrowIfNullOrWhiteSpace(location, nameof(location));

        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        }

        if (profileId == Guid.Empty)
        {
            throw new ArgumentException("Profile ID cannot be empty.", nameof(profileId));
        }

        ConferenceId = conferenceId;
        ConferenceName = conferenceName;
        Location = location;
        ImageUrl = imageUrl;
        StartDate = startDate;
        EndDate = endDate;
        ProfileId = profileId;
        IsFollowing = isFollowing;
        IsAttending = isAttending;

        if (presentations != null)
        {
            _presentations.AddRange(presentations);
        }
    }

    public void AddPresentation(PresentationPresence presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        if (_presentations.Any(p => p.PresentationId == presentation.PresentationId))
        {
            throw new InvalidOperationException($"Presentation with ID {presentation.PresentationId} already exists.");
        }

        _presentations.Add(presentation);
    }

    public void UpdateAttendance(bool isAttending)
    {
        IsAttending = isAttending;
    }

    /// <summary>
    /// Updates conference details when conference properties change.
    /// </summary>
    /// <param name="conferenceName">The updated conference name.</param>
    /// <param name="location">The updated location.</param>
    /// <param name="startDate">The updated start date.</param>
    /// <param name="endDate">The updated end date.</param>
    /// <param name="imageUrl">The updated image URL (optional).</param>
    public void UpdateConferenceDetails(
        string conferenceName,
        string location,
        DateOnly startDate,
        DateOnly endDate,
        string? imageUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conferenceName, nameof(conferenceName));
        ArgumentException.ThrowIfNullOrWhiteSpace(location, nameof(location));

        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        }

        ConferenceName = conferenceName;
        Location = location;
        StartDate = startDate;
        EndDate = endDate;
        ImageUrl = imageUrl;
    }
}
