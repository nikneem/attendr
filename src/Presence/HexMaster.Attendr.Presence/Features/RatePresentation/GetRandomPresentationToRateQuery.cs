namespace HexMaster.Attendr.Presence.Features.RatePresentation;

/// <summary>
/// Query to retrieve an unrated presentation at a specific index.
/// </summary>
/// <param name="ProfileId">The unique identifier of the profile.</param>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
/// <param name="Index">The index (0, 1, or 2) indicating which unrated presentation to retrieve.</param>
public sealed record GetRandomPresentationToRateQuery(Guid ProfileId, Guid ConferenceId, int Index);
