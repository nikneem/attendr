using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Presence.Features.SetPreferredPresentation;

/// <summary>
/// Command to set a presentation as the preferred session for a given timeslot.
/// </summary>
/// <param name="ProfileId">The unique identifier of the profile.</param>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
/// <param name="PresentationId">The unique identifier of the presentation to set as preferred.</param>
public sealed record SetPreferredPresentationCommand(
    Guid ProfileId,
    Guid ConferenceId,
    Guid PresentationId) : IAttendrCommand;
