using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Presence.Features.CreateConferencePresence;

/// <summary>
/// Command to create conference presence records for one or more profiles.
/// </summary>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
/// <param name="ProfileIds">The collection of profile IDs to create presence records for.</param>
public sealed record CreateConferencePresenceCommand(
    Guid ConferenceId,
    IEnumerable<Guid> ProfileIds) : IAttendrCommand;
