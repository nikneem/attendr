using HexMaster.Attendr.Core.DomainEvents;

namespace HexMaster.Attendr.Groups.Abstractions.DomainEvents;

/// <summary>
/// Domain event raised when a member is removed from a group.
/// </summary>
public sealed record MemberDeletedEvent : DomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the group.
    /// </summary>
    public required Guid GroupId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the profile that was removed.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the profile that removed the member.
    /// May be null if the member removed themselves.
    /// </summary>
    public Guid? RemovedByProfileId { get; init; }
}
