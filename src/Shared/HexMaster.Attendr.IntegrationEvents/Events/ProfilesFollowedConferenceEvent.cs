namespace HexMaster.Attendr.IntegrationEvents.Events;

public sealed class ProfilesFollowedConferenceEvent : IntegrationEvent
{
    public override string EventType => "profiles.followed.conference";

    public Guid ConferenceId { get; init; }
    public IReadOnlyCollection<Guid> ProfileIds { get; init; } = Array.Empty<Guid>();
}
