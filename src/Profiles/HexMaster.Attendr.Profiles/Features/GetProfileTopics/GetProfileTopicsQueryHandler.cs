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

        var now = DateTimeOffset.UtcNow;
        var maxAgeDate = now.AddMonths(-TopicWeightConstants.MaxOccasionAgeMonths);

        return topics
            .Select(topic =>
            {
                // Manual topics always have a weight of 100
                if (topic.IsManual)
                {
                    return new ProfileTopicDto(
                        topic.Id,
                        topic.ProfileId,
                        topic.TopicKey,
                        topic.TopicName,
                        topic.IsManual,
                        topic.CreatedOn,
                        100);
                }

                // Filter occasions within the max age timespan
                var relevantOccasions = topic.Occasions
                    .Where(o => o.Date >= maxAgeDate)
                    .ToList();

                // Calculate total weight with exponential decay applied to each occasion
                var totalDecayedWeight = relevantOccasions
                    .Sum(o => _decayService.CalculateDecayedWeight(o.Weight, o.Date, now));

                // Cap the total weight at 100
                var cappedWeight = Math.Min(totalDecayedWeight, 100);

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
