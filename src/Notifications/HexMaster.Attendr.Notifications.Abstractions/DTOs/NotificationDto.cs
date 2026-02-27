using HexMaster.Attendr.Notifications.Abstractions.Enums;

namespace HexMaster.Attendr.Notifications.Abstractions.DTOs;

/// <summary>
/// DTO for notification response.
/// </summary>
public sealed class NotificationDto
{
    public required Guid Id { get; init; }
    public required Guid ProfileId { get; init; }
    public required string TypeKey { get; init; }
    public required string Severity { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public string? Url { get; init; }
    public Guid? ActorId { get; init; }
    public Dictionary<string, string>? EntityRefs { get; init; }
    public int Count { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastOccurredAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
    public bool IsRead => ReadAt.HasValue;
    public Dictionary<string, ChannelDeliveryDto>? ChannelDeliveries { get; init; }
}

public sealed class ChannelDeliveryDto
{
    public required bool Enabled { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset? DeliveredAt { get; init; }
    public string? ErrorMessage { get; init; }
}
