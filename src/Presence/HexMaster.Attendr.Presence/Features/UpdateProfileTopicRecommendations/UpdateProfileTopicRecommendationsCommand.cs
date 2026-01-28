using HexMaster.Attendr.IntegrationEvents.Events.Profiles;

namespace HexMaster.Attendr.Presence.Features.UpdateProfileTopicRecommendations;

/// <summary>
/// Command to update presentation recommendations based on profile topic changes.
/// </summary>
public sealed record UpdateProfileTopicRecommendationsCommand(ProfileTopicsChangedEvent Event);
