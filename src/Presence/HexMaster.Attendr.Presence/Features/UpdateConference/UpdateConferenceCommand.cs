using HexMaster.Attendr.IntegrationEvents.Events.Conferences;

namespace HexMaster.Attendr.Presence.Features.UpdateConference;

/// <summary>
/// Command to update conference presence records when conference details change.
/// </summary>
/// <param name="Event">The conference updated event containing updated conference properties.</param>
public sealed record UpdateConferenceCommand(ConferenceUpdatedEvent Event);
