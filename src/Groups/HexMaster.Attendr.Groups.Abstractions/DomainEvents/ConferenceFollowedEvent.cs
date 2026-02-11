using HexMaster.Attendr.Core.DomainEvents;

namespace HexMaster.Attendr.Groups.Abstractions.DomainEvents;

/// <summary>
/// Domain event raised when a group starts following a conference.
/// </summary>
public sealed record ConferenceFollowedEvent : DomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the group.
    /// </summary>
    public required Guid GroupId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the conference being followed.
    /// </summary>
    public required Guid ConferenceId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the profile that initiated following the conference.
    /// </summary>
    public required Guid InitiatedByProfileId { get; init; }
}
