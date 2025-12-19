using HexMaster.Attendr.Profiles.DomainModels;

namespace HexMaster.Attendr.Profiles.Repositories;

/// <summary>
/// Repository interface for Profile aggregate root operations.
/// </summary>
public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<Profile?> GetBySubjectIdAsync(string subjectId, CancellationToken cancellationToken = default);

    Task AddAsync(Profile profile, CancellationToken cancellationToken = default);

    Task UpdateAsync(Profile profile, CancellationToken cancellationToken = default);
}
