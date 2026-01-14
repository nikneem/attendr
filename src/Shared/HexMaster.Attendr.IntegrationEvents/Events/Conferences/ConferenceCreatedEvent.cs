using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events.Conferences;

public sealed class ConferenceCreatedEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.ConferenceCreated;

    public Guid ConferenceId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}
