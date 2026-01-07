using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events;

/// <summary>
/// Integration event published when a profile changes their attendance status for a conference.
/// </summary>
public sealed class ProfileConferenceAttendanceChangedEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.ProfileConferenceAttendanceChanged;

    public Guid ProfileId { get; init; }
    public Guid ConferenceId { get; init; }
    public string ConferenceName { get; init; } = string.Empty;
    public bool IsAttending { get; init; }
}
