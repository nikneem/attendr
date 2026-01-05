using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence.Services;

public interface IPresentationPresenceRepository
{
    Task<IReadOnlyCollection<PresentationPresence>> GetByConferenceAndPresentationAsync(
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid profileId,
        Guid conferenceId,
        PresentationPresence presentation,
        CancellationToken cancellationToken = default);
}
