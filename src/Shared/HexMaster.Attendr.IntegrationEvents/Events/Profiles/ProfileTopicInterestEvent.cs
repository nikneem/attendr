using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events.Profiles;

/// <summary>
/// Event published when a profile shows interest in a topic.
/// This can be triggered by explicit user action or inferred from session attendance and engagement.
/// </summary>
public sealed class ProfileTopicInterestEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.ProfileTopicInterest;

    /// <summary>
    /// The profile ID who showed interest in the topic.
    /// </summary>
    public required string ProfileId { get; init; }

    /// <summary>
    /// The topic key (normalized lowercase identifier).
    /// </summary>
    public required string TopicKey { get; init; }

    /// <summary>
    /// The topic name (display name).
    /// </summary>
    public required string TopicName { get; init; }

    /// <summary>
    /// The interest weight (0-100).
    /// Higher values indicate stronger interest based on engagement.
    /// </summary>
    public required int Weight { get; init; }

    /// <summary>
    /// Indicates if this interest was manually set by the user (true) or automatically inferred (false).
    /// </summary>
    public bool IsManual { get; init; }
}
