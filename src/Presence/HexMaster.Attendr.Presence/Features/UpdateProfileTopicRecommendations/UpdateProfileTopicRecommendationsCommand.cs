namespace HexMaster.Attendr.Presence.Features.UpdateProfileTopicRecommendations;

/// <summary>
/// Command to update presentation recommendations based on profile topic changes.
/// </summary>
public sealed record UpdateProfileTopicRecommendationsCommand(
    Guid ProfileId,
    IReadOnlyList<ProfileTopicWeight> Topics);

/// <summary>
/// Represents a profile topic with its weight.
/// </summary>
public sealed record ProfileTopicWeight(
    string TopicKey,
    int Weight);
