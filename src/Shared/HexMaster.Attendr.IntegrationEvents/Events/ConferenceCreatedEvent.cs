namespace HexMaster.Attendr.IntegrationEvents.Events;

public sealed class ConferenceCreatedEvent : IntegrationEvent
{
    public override string EventType => "conference.created";

    public Guid ConferenceId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}
