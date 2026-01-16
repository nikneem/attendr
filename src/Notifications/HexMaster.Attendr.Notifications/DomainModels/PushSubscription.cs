using HexMaster.Attendr.Notifications.Abstractions.DomainModels;

namespace HexMaster.Attendr.Notifications.DomainModels;

/// <summary>
/// Concrete push subscription domain model.
/// </summary>
public sealed class PushSubscription : IPushSubscription
{
    public required Guid ProfileId { get; init; }
    public required string Endpoint { get; init; }
    public required string P256dh { get; init; }
    public required string Auth { get; init; }
    public required string UserAgent { get; init; }
    public DateTime? ExpirationTime { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
