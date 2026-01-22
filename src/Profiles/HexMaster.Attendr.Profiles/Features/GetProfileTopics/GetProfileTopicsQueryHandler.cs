using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Repositories;

namespace HexMaster.Attendr.Profiles.Features.GetProfileTopics;

public sealed class GetProfileTopicsQueryHandler : IQueryHandler<GetProfileTopicsQuery, IReadOnlyList<ProfileTopicDto>>
{
    private readonly IProfileTopicRepository _repository;

    public GetProfileTopicsQueryHandler(IProfileTopicRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProfileTopicDto>> Handle(GetProfileTopicsQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.ProfileId))
        {
            throw new ArgumentException("ProfileId is required.", nameof(query.ProfileId));
        }

        var topics = await _repository.GetByProfileIdAsync(query.ProfileId, cancellationToken);

        return topics
            .Select(topic => new ProfileTopicDto(
                topic.Id,
                topic.ProfileId,
                topic.TopicKey,
                topic.TopicName,
                topic.IsManual,
                topic.CreatedOn,
                topic.Occasions
                    .Select(o => new ProfileTopicOccasionDto(o.Weight, o.Date))
                    .ToArray()))
            .ToArray();
    }
}
