using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Features.GetProfileTopics;
using HexMaster.Attendr.Profiles.Repositories;
using Moq;

namespace HexMaster.Attendr.Profiles.Tests.Features.GetProfileTopics;

public class GetProfileTopicsQueryHandlerTests
{
    private readonly Mock<IProfileTopicRepository> _repository = new();
    private readonly GetProfileTopicsQueryHandler _handler;

    public GetProfileTopicsQueryHandlerTests()
    {
        _handler = new GetProfileTopicsQueryHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProfileIdIsMissing()
    {
        var query = new GetProfileTopicsQuery(string.Empty);
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldMapTopics_WhenFound()
    {
        var profileId = Guid.NewGuid().ToString();
        var topicId = Guid.NewGuid().ToString();
        var occasions = new[] { new Occasion(10, DateTimeOffset.UtcNow) };
        var topics = new List<ProfileTopic>
        {
            ProfileTopic.FromPersisted(
                topicId,
                profileId,
                "topic-key",
                "Topic Name",
                true,
                occasions,
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow)
        };

        _repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topics);

        var result = await _handler.Handle(new GetProfileTopicsQuery(profileId), CancellationToken.None);

        Assert.Single(result);
        var item = result[0];
        Assert.Equal(topicId, item.Id);
        Assert.Equal(profileId, item.ProfileId);
        Assert.Equal("topic-key", item.TopicKey);
        Assert.Equal("Topic Name", item.TopicName);
        Assert.True(item.IsManual);
        Assert.Single(item.Occasions);
        Assert.Equal(10, item.Occasions.First().Weight);
    }
}
