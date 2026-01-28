using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events.Profiles;

/// <summary>
/// Integration event published when a profile's topics have changed.
/// This can occur when topics are created, updated, or when manual status is toggled.
/// </summary>
public sealed class ProfileTopicsChangedEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.ProfileTopicsChanged;

    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<ProfileTopicInfo> Topics { get; init; } = Array.Empty<ProfileTopicInfo>();
}

/// <summary>
/// Represents a profile topic with its calculated weight.
/// </summary>
public sealed record ProfileTopicInfo(
    string TopicKey,
    string TopicName,
    int Weight);
