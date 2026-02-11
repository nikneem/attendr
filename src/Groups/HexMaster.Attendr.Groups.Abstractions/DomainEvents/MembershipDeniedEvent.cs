using HexMaster.Attendr.Core.DomainEvents;

namespace HexMaster.Attendr.Groups.Abstractions.DomainEvents;

/// <summary>
/// Domain event raised when a membership request is denied.
/// </summary>
public sealed record MembershipDeniedEvent : DomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the group.
    /// </summary>
    public required Guid GroupId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the profile whose request was denied.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the join request that was denied.
    /// </summary>
    public required Guid JoinRequestId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the profile that denied the request.
    /// </summary>
    public required Guid DeniedByProfileId { get; init; }

    /// <summary>
    /// Gets an optional reason for denying the request.
    /// </summary>
    public string? Reason { get; init; }
}
