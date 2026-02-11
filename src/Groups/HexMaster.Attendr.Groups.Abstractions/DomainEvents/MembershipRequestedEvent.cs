using HexMaster.Attendr.Core.DomainEvents;

namespace HexMaster.Attendr.Groups.Abstractions.DomainEvents;

/// <summary>
/// Domain event raised when a profile requests to join a group.
/// </summary>
public sealed record MembershipRequestedEvent : DomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the group.
    /// </summary>
    public required Guid GroupId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the profile requesting membership.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the join request.
    /// </summary>
    public required Guid JoinRequestId { get; init; }

    /// <summary>
    /// Gets an optional message provided with the membership request.
    /// </summary>
    public string? Message { get; init; }
}
