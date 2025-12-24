namespace HexMaster.Attendr.Presence.DomainModels;

public sealed class ConferencePresence
{
    public Guid ConferenceId { get; private set; }
    public string ConferenceName { get; private set; }
    public string Location { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public Guid ProfileId { get; private set; }

    private readonly List<PresentationPresence> _presentations = new();
    public IReadOnlyCollection<PresentationPresence> Presentations => _presentations.AsReadOnly();

    private ConferencePresence()
    {
        ConferenceId = Guid.Empty;
        ConferenceName = string.Empty;
        Location = string.Empty;
        StartDate = DateOnly.MinValue;
        EndDate = DateOnly.MinValue;
        ProfileId = Guid.Empty;
    }

    public ConferencePresence(
        Guid conferenceId,
        string conferenceName,
        string location,
        DateOnly startDate,
        DateOnly endDate,
        Guid profileId,
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
        StartDate = startDate;
        EndDate = endDate;
        ProfileId = profileId;

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
}
