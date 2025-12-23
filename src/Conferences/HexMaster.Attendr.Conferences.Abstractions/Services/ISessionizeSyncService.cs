namespace HexMaster.Attendr.Conferences.Abstractions.Services;

/// <summary>
/// Service for synchronizing conference data from Sessionize.
/// </summary>
public interface ISessionizeSyncService
{
    /// <summary>
    /// Synchronizes conference data from Sessionize API.
    /// </summary>
    /// <param name="conferenceId">The ID of the conference to synchronize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the synchronization operation, returning the sync result or null if conference not found.</returns>
    Task<SessionizeSyncResult?> SynchronizeConferenceAsync(Guid conferenceId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a Sessionize synchronization operation.
/// </summary>
public sealed record SessionizeSyncResult(
    Guid ConferenceId,
    int SpeakersCount,
    int RoomsCount,
    int PresentationsCount);
