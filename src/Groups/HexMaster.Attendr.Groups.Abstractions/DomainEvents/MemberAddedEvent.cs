using HexMaster.Attendr.Core.DomainEvents;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;

namespace HexMaster.Attendr.Groups.Abstractions.DomainEvents;

/// <summary>
/// Domain event raised when a member is added to a group.
/// </summary>
public sealed record MemberAddedEvent : DomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the group.
    /// </summary>
    public required Guid GroupId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the profile that was added as a member.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// Gets the role assigned to the member.
    /// </summary>
    public required GroupRole Role { get; init; }

    /// <summary>
    /// Gets the unique identifier of the profile that added the member.
    /// </summary>
    public Guid? AddedByProfileId { get; init; }
}
