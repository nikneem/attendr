using HexMaster.Attendr.Groups.Abstractions.DomainModels;

namespace HexMaster.Attendr.Groups.DomainModels;

/// <summary>
/// Domain model representing a group check-in to a presentation.
/// </summary>
public sealed class CheckIn : ICheckIn
{
    public Guid Id { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid ConferenceId { get; private set; }
    public Guid PresentationId { get; private set; }
    public IPresentationData PresentationData { get; private set; }

    public DateTimeOffset Expiration { get; private set; }

    private readonly List<CheckedInMember> _members = new();
    public IReadOnlyCollection<CheckedInMember> Members => _members.AsReadOnly();

    IReadOnlyCollection<ICheckedInMember> ICheckIn.Members => _members.AsReadOnly();

    private CheckIn(
        Guid id,
        Guid groupId,
        Guid conferenceId,
        Guid presentationId,
        PresentationData presentationData,
        DateTimeOffset expiration,
        IEnumerable<CheckedInMember>? members = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Check-in ID cannot be empty.", nameof(id));
        }

        if (groupId == Guid.Empty)
        {
            throw new ArgumentException("Group ID cannot be empty.", nameof(groupId));
        }

        if (conferenceId == Guid.Empty)
        {
            throw new ArgumentException("Conference ID cannot be empty.", nameof(conferenceId));
        }

        if (presentationId == Guid.Empty)
        {
            throw new ArgumentException("Presentation ID cannot be empty.", nameof(presentationId));
        }

        ArgumentNullException.ThrowIfNull(presentationData);

        Id = id;
        GroupId = groupId;
        ConferenceId = conferenceId;
        PresentationId = presentationId;
        PresentationData = presentationData;
        Expiration = expiration;

        if (members != null)
        {
            _members.AddRange(members);
        }
    }

    public static CheckIn Create(
        Guid groupId,
        Guid conferenceId,
        Guid presentationId,
        PresentationData presentationData,
        DateTimeOffset expiration)
    {
        return new CheckIn(Guid.NewGuid(), groupId, conferenceId, presentationId, presentationData, expiration);
    }

    public static CheckIn FromPersisted(
        Guid id,
        Guid groupId,
        Guid conferenceId,
        Guid presentationId,
        PresentationData presentationData,
        DateTimeOffset expiration,
        IEnumerable<CheckedInMember>? members = null)
    {
        return new CheckIn(id, groupId, conferenceId, presentationId, presentationData, expiration, members);
    }

    public void AddMember(CheckedInMember member)
    {
        ArgumentNullException.ThrowIfNull(member);

        if (_members.Any(m => m.Id == member.Id))
        {
            throw new InvalidOperationException($"Member {member.Id} is already checked in.");
        }

        _members.Add(member);
    }

    void ICheckIn.AddMember(ICheckedInMember member)
    {
        if (member is CheckedInMember checkedInMember)
        {
            AddMember(checkedInMember);
        }
        else
        {
            throw new ArgumentException("Member must be of type CheckedInMember", nameof(member));
        }
    }

    public void RemoveMember(Guid memberId)
    {
        var member = _members.FirstOrDefault(m => m.Id == memberId);
        if (member != null)
        {
            _members.Remove(member);
        }
    }

    public bool IsExpired(DateTimeOffset now)
    {
        return Expiration <= now;
    }
}

public sealed class PresentationData : IPresentationData
{
    public Guid Id { get; }
    public string Title { get; }
    public string Abstract { get; }
    public string Room { get; }
    public DateTime StartDateTime { get; }
    public DateTime EndDateTime { get; }
    public IReadOnlyCollection<PresentationSpeaker> Speakers { get; }

    IReadOnlyCollection<IPresentationSpeaker> IPresentationData.Speakers => Speakers;

    public PresentationData(
        Guid id,
        string title,
        string @abstract,
        string room,
        DateTime startDateTime,
        DateTime endDateTime,
        IEnumerable<PresentationSpeaker> speakers)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Presentation ID cannot be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(@abstract, nameof(@abstract));
        ArgumentException.ThrowIfNullOrWhiteSpace(room, nameof(room));

        Id = id;
        Title = title;
        Abstract = @abstract;
        Room = room;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        Speakers = speakers?.ToList().AsReadOnly() ?? Array.Empty<PresentationSpeaker>().ToList().AsReadOnly();
    }
}

public sealed class PresentationSpeaker : IPresentationSpeaker
{
    public Guid Id { get; }
    public string Name { get; }
    public string? ProfilePictureUrl { get; }

    public PresentationSpeaker(Guid id, string name, string? profilePictureUrl)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Speaker ID cannot be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        Id = id;
        Name = name;
        ProfilePictureUrl = profilePictureUrl;
    }
}

public sealed class CheckedInMember : ICheckedInMember
{
    public Guid Id { get; }
    public string Name { get; }
    public string? ProfilePictureUrl { get; }

    public CheckedInMember(Guid id, string name, string? profilePictureUrl)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Member ID cannot be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        Id = id;
        Name = name;
        ProfilePictureUrl = profilePictureUrl;
    }
}
