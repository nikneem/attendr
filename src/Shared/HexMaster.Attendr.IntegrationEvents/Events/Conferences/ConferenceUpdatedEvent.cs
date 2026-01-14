using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events.Conferences;

public sealed class ConferenceUpdatedEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.ConferenceUpdated;

    public Guid ConferenceId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string? ImageUrl { get; init; }
}
