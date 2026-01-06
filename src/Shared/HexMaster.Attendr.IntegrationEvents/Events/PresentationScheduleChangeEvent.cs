using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events;

/// <summary>
/// Integration event published when a presentation's schedule changes and the profile has favorited it.
/// This event triggers notifications to attendees about schedule changes for their favorite presentations.
/// </summary>
public sealed class PresentationScheduleChangeEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.PresentationScheduleChanged;

    public Guid ConferenceId { get; set; }
    public Guid PresentationId { get; set; }
    public Guid ProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
}
