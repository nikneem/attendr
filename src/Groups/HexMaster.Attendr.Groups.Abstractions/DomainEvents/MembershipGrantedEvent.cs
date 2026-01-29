using HexMaster.Attendr.Core.DomainEvents;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;

namespace HexMaster.Attendr.Groups.Abstractions.DomainEvents;

/// <summary>
/// Domain event raised when a membership request is approved and the profile is granted membership.
/// </summary>
public sealed record MembershipGrantedEvent : DomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the group.
    /// </summary>
    public required Guid GroupId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the profile that was granted membership.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the join request that was approved.
    /// </summary>
    public required Guid JoinRequestId { get; init; }

    /// <summary>
    /// Gets the role assigned to the new member.
    /// </summary>
    public required GroupRole Role { get; init; }

    /// <summary>
    /// Gets the unique identifier of the profile that approved the request.
    /// </summary>
    public required Guid ApprovedByProfileId { get; init; }
}
