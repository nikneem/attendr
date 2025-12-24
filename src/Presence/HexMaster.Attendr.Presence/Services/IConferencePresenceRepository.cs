using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence.Services;

public interface IConferencePresenceRepository
{
    Task<bool> ExistsAsync(Guid profileId, Guid conferenceId, CancellationToken cancellationToken = default);
    Task AddAsync(ConferencePresence presence, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ConferencePresence>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);
}
