using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events.Profiles;

public sealed class ProfileCreatedEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.ProfileCreated;

    public string ProfileId { get; init; } = string.Empty;
    public string SubjectId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
}
