namespace HexMaster.Attendr.Notifications.Abstractions.DTOs;

public sealed class NotificationTypeDto
{
    public required string TypeKey { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string Severity { get; init; }
    public required bool AllowsStacking { get; init; }
    public Dictionary<string, bool>? DefaultChannelSettings { get; init; }
}
