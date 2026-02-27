namespace HexMaster.Attendr.Notifications.Abstractions.DomainModels;

/// <summary>
/// Represents a push subscription for a profile.
/// </summary>
public interface IPushSubscription
{
    Guid ProfileId { get; }
    string Endpoint { get; }
    string P256dh { get; }
    string Auth { get; }
    string UserAgent { get; }
    DateTimeOffset? ExpirationTime { get; }
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
}
