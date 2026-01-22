using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Repositories;

namespace HexMaster.Attendr.Profiles.Features.GetProfileTopics;

public sealed class GetProfileTopicsQueryHandler : IQueryHandler<GetProfileTopicsQuery, IReadOnlyList<ProfileTopicDto>>
{
    private readonly IProfileTopicRepository _topicRepository;
    private readonly IProfileRepository _profileRepository;

    public GetProfileTopicsQueryHandler(
        IProfileTopicRepository topicRepository,
        IProfileRepository profileRepository)
    {
        _topicRepository = topicRepository;
        _profileRepository = profileRepository;
    }

    public async Task<IReadOnlyList<ProfileTopicDto>> Handle(GetProfileTopicsQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.SubjectId))
        {
            throw new ArgumentException("SubjectId is required.", nameof(query.SubjectId));
        }

        var profile = await _profileRepository.GetBySubjectIdAsync(query.SubjectId, cancellationToken);
        if (profile is null)
        {
            return Array.Empty<ProfileTopicDto>();
        }

        var topics = await _topicRepository.GetByProfileIdAsync(profile.Id, cancellationToken);

        var threeYearsAgo = DateTimeOffset.UtcNow.AddYears(-3);

        return topics
            .Select(topic =>
            {
                var recentOccasions = topic.Occasions
                    .Where(o => o.Date >= threeYearsAgo)
                    .ToList();

                var totalWeight = recentOccasions.Sum(o => o.Weight);
                var cappedWeight = Math.Min(totalWeight, 100);

                return new ProfileTopicDto(
                    topic.Id,
                    topic.ProfileId,
                    topic.TopicKey,
                    topic.TopicName,
                    topic.IsManual,
                    topic.CreatedOn,
                    cappedWeight);
            })
            .ToArray();
    }
}
