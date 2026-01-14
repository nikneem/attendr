using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;

namespace HexMaster.Attendr.Presence.Features.UpdatePresentation;

/// <summary>
/// Command to update presentation information when presentations are modified.
/// Triggered by PresentationUpdatedEvent from the Conferences service.
/// </summary>
/// <param name="Event">The presentation updated event containing updated presentation details.</param>
public sealed record UpdatePresentationCommand(PresentationUpdatedEvent Event) : IAttendrCommand;
