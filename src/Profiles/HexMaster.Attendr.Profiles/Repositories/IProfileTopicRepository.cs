using HexMaster.Attendr.Profiles.DomainModels;

namespace HexMaster.Attendr.Profiles.Repositories;

public interface IProfileTopicRepository
{
    Task<IReadOnlyList<ProfileTopic>> GetByProfileIdAsync(string profileId, CancellationToken cancellationToken = default);

    Task<ProfileTopic?> GetByProfileIdAndKeyAsync(string profileId, string topicKey, CancellationToken cancellationToken = default);

    Task UpsertAsync(ProfileTopic topic, CancellationToken cancellationToken = default);
}
