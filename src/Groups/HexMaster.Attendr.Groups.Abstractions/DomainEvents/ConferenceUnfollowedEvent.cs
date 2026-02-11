using HexMaster.Attendr.Core.DomainEvents;

namespace HexMaster.Attendr.Groups.Abstractions.DomainEvents;

/// <summary>
/// Domain event raised when a group stops following a conference.
/// </summary>
public sealed record ConferenceUnfollowedEvent : DomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the group.
    /// </summary>
    public required Guid GroupId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the conference that is no longer being followed.
    /// </summary>
    public required Guid ConferenceId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the profile that initiated unfollowing the conference.
    /// </summary>
    public required Guid InitiatedByProfileId { get; init; }
}
