using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Constants;
using HexMaster.Attendr.Profiles.Repositories;
using HexMaster.Attendr.Profiles.Services;

namespace HexMaster.Attendr.Profiles.Features.GetProfileTopics;

public sealed class GetProfileTopicsQueryHandler : IQueryHandler<GetProfileTopicsQuery, IReadOnlyList<ProfileTopicDto>>
{
    private readonly IProfileTopicRepository _topicRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly TopicWeightDecayService _decayService;

    public GetProfileTopicsQueryHandler(
        IProfileTopicRepository topicRepository,
        IProfileRepository profileRepository,
        TopicWeightDecayService decayService)
    {
        _topicRepository = topicRepository;
        _profileRepository = profileRepository;
        _decayService = decayService;
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

        return topics
            .Select(topic =>
            {
                var occasions = topic.Occasions.Select(o => (o.Weight, o.Date));
                var totalWeight = _decayService.CalculateTopicWeight(topic.IsManual, occasions);

                return new ProfileTopicDto(
                    topic.Id,
                    topic.ProfileId,
                    topic.TopicKey,
                    topic.TopicName,
                    topic.IsManual,
                    topic.CreatedOn,
                    totalWeight);
            })
            .ToArray();
    }
}
