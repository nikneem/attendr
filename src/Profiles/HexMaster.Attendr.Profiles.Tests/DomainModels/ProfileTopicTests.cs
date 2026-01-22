using Bogus;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.DomainModels;

namespace HexMaster.Attendr.Profiles.Tests.DomainModels;

public class ProfileTopicTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Create_ShouldNormalizeValues_AndSetOccasions()
    {
        var profileId = _faker.Random.Guid().ToString();
        var topicKey = "CloudNative";
        var topicName = "Cloud Native";
        var occasion = new Occasion(80, DateTimeOffset.UtcNow.AddDays(-1));

        var topic = ProfileTopic.Create(profileId, topicKey, topicName, true, new[] { occasion });

        Assert.Equal(profileId, topic.ProfileId);
        Assert.Equal(topicKey.ToLowerInvariant(), topic.TopicKey);
        Assert.Equal(topicName, topic.TopicName);
        Assert.True(topic.IsManual);
        Assert.Single(topic.Occasions);
    }

    [Fact]
    public void AddOccasion_ShouldAppendAndUpdateModifiedOn()
    {
        var topic = ProfileTopic.Create(_faker.Random.Guid().ToString(), "ai", "AI", false);
        var before = topic.ModifiedOn;

        topic.AddOccasion(50, DateTimeOffset.UtcNow);

        Assert.Single(topic.Occasions);
        Assert.NotEqual(before, topic.ModifiedOn);
    }

    [Fact]
    public void AddOccasion_ShouldThrow_WhenWeightIsOutOfRange()
    {
        var topic = ProfileTopic.Create(_faker.Random.Guid().ToString(), "ai", "AI", false);

        Assert.Throws<DomainException>(() => topic.AddOccasion(-1, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => topic.AddOccasion(101, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void SetTopicName_ShouldUpdateValue()
    {
        var topic = ProfileTopic.Create(_faker.Random.Guid().ToString(), "ai", "AI", false);
        var newName = "Artificial Intelligence";

        topic.SetTopicName(newName);

        Assert.Equal(newName, topic.TopicName);
        Assert.NotNull(topic.ModifiedOn);
    }

    [Fact]
    public void SetIsManual_ShouldUpdateFlag()
    {
        var topic = ProfileTopic.Create(_faker.Random.Guid().ToString(), "ai", "AI", false);

        topic.SetIsManual(true);

        Assert.True(topic.IsManual);
        Assert.NotNull(topic.ModifiedOn);
    }
}
