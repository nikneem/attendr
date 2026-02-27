namespace HexMaster.Attendr.Notifications.Abstractions.DTOs;

public sealed class NotificationPreferencesDto
{
    public required Guid ProfileId { get; init; }
    public Dictionary<string, Dictionary<string, bool>>? TypeChannelPreferences { get; init; }
    public DateTimeOffset? DoNotDisturbUntil { get; init; }
}
