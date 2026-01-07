using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events;

/// <summary>
/// Integration event published when a profile checks in or out of a presentation.
/// </summary>
public sealed class ProfileCheckedInEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.ProfileCheckedIn;

    public Guid ConferenceId { get; init; }
    public Guid PresentationId { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime StartDateTime { get; init; }
    public DateTime EndDateTime { get; init; }
    public string Room { get; init; } = string.Empty;
    public Guid ProfileId { get; init; }
    public bool IsCheckedIn { get; init; }
}
