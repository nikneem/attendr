using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence.Services;

public interface IPresentationPresenceRepository
{
    Task<IReadOnlyCollection<PresentationPresence>> GetByConferenceAndPresentationAsync(
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default);

    Task<PresentationPresence?> GetByIdAsync(
        Guid profileId,
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PresentationPresence>> GetUnratedByProfileAndConferenceAsync(
        Guid profileId,
        Guid conferenceId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid profileId,
        Guid conferenceId,
        PresentationPresence presentation,
        CancellationToken cancellationToken = default);
}
