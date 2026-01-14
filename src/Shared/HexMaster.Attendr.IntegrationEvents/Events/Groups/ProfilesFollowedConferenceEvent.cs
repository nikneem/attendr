using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events.Groups;

public sealed class ProfilesFollowedConferenceEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.ProfilesFollowedConference;

    public Guid ConferenceId { get; init; }
    public IReadOnlyCollection<Guid> ProfileIds { get; init; } = Array.Empty<Guid>();
}
