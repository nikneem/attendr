using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.DeleteConference;

/// <summary>
/// Command to delete a conference.
/// </summary>
/// <param name="Id">The unique identifier of the conference to delete.</param>
public sealed record DeleteConferenceCommand(Guid Id) : IAttendrCommand;
