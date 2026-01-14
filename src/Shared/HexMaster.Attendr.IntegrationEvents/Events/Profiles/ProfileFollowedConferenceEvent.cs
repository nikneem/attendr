using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events.Profiles;

public sealed class ProfileFollowedConferenceEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.ProfileFollowedConference;

    public Guid ConferenceId { get; init; }
    public Guid ProfileId { get; init; }
}
