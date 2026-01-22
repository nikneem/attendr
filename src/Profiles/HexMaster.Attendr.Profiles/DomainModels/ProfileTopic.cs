using System.Collections.ObjectModel;
using HexMaster.Attendr.Core.DomainModels;
using HexMaster.Attendr.Core.Exceptions;

namespace HexMaster.Attendr.Profiles.DomainModels;

public sealed class ProfileTopic : StatefulDomainModel<string>
{
    private readonly List<Occasion> _occasions = new();

    public string ProfileId { get; private set; }
    public string TopicKey { get; private set; }
    public string TopicName { get; private set; }
    public bool IsManual { get; private set; }
    public IReadOnlyCollection<Occasion> Occasions => new ReadOnlyCollection<Occasion>(_occasions);

    private ProfileTopic(
        string id,
        string profileId,
        string topicKey,
        string topicName,
        bool isManual,
        IEnumerable<Occasion> occasions,
        DomainModelState initialState = DomainModelState.Pristine) : base(id, initialState)
    {
        ProfileId = NormalizeProfileId(profileId);
        TopicKey = NormalizeTopicKey(topicKey);
        TopicName = NormalizeTopicName(topicName);
        IsManual = isManual;

        foreach (var occasion in occasions)
        {
            _occasions.Add(occasion);
        }
    }

    public static ProfileTopic Create(
        string profileId,
        string topicKey,
        string topicName,
        bool isManual,
        IEnumerable<Occasion>? occasions = null)
    {
        var topicId = Guid.NewGuid().ToString();
        var normalizedOccasions = occasions?.ToList() ?? new List<Occasion>();

        return new ProfileTopic(topicId, profileId, topicKey, topicName, isManual, normalizedOccasions, DomainModelState.Created);
    }

    public static ProfileTopic FromPersisted(
        string id,
        string profileId,
        string topicKey,
        string topicName,
        bool isManual,
        IEnumerable<Occasion> occasions,
        DateTimeOffset createdOn,
        DateTimeOffset? modifiedOn)
    {
        var topic = new ProfileTopic(id, profileId, topicKey, topicName, isManual, occasions);
        topic.SetCreatedOn(createdOn);
        topic.SetModifiedOn(modifiedOn);
        return topic;
    }

    public void SetTopicName(string topicName)
    {
        var normalized = NormalizeTopicName(topicName);
        if (ShouldUpdateProperty(TopicName, normalized))
        {
            TopicName = normalized;
            UpdateModifiedOn();
        }
    }

    public void SetIsManual(bool isManual)
    {
        if (ShouldUpdateProperty(IsManual, isManual))
        {
            IsManual = isManual;
            UpdateModifiedOn();
        }
    }

    public void AddOccasion(int weight, DateTimeOffset date)
    {
        var occasion = new Occasion(weight, date);
        _occasions.Add(occasion);
        UpdateModifiedOn();
    }

    private static string NormalizeProfileId(string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new DomainException("ProfileId cannot be empty.");
        }

        return profileId.Trim();
    }

    private static string NormalizeTopicKey(string topicKey)
    {
        if (string.IsNullOrWhiteSpace(topicKey))
        {
            throw new DomainException("TopicKey cannot be empty.");
        }

        return topicKey.Trim().ToLowerInvariant();
    }

    private static string NormalizeTopicName(string topicName)
    {
        if (string.IsNullOrWhiteSpace(topicName))
        {
            throw new DomainException("TopicName cannot be empty.");
        }

        return topicName.Trim();
    }
}

public sealed record Occasion
{
    public int Weight { get; }
    public DateTimeOffset Date { get; }

    public Occasion(int weight, DateTimeOffset date)
    {
        ValidateWeight(weight);
        Weight = weight;
        Date = date;
    }

    private static void ValidateWeight(int weight)
    {
        if (weight is < 0 or > 100)
        {
            throw new DomainException("Weight must be between 0 and 100.");
        }
    }
}
