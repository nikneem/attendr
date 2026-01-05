namespace HexMaster.Attendr.Presence.Features.RatePresentation;

/// <summary>
/// Query to retrieve a random unrated presentation for a profile at a specific conference.
/// </summary>
/// <param name="ProfileId">The unique identifier of the profile.</param>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
public sealed record GetRandomPresentationToRateQuery(Guid ProfileId, Guid ConferenceId);
