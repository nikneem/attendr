namespace HexMaster.Attendr.IntegrationEvents.Events;

public sealed class ProfileFollowedConferenceEvent : IntegrationEvent
{
    public override string EventType => "profile.followed.conference";

    public Guid ConferenceId { get; init; }
    public Guid ProfileId { get; init; }
}
