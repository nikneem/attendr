using Bogus;
using HexMaster.Attendr.Profiles.DomainModels;
using System.Text.Json;

namespace HexMaster.Attendr.Profiles.Tests.DataAccess;

public class ProfileTopicMapperTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Serialization_ShouldHandleOccasionsAsJson()
    {
        var occasion = new Occasion(75, DateTimeOffset.UtcNow.AddDays(-1));
        var occasions = new[] { occasion };

        var json = JsonSerializer.Serialize(occasions, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.NotEmpty(json);
        var deserialized = JsonSerializer.Deserialize<IReadOnlyCollection<Occasion>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        Assert.NotNull(deserialized);
        Assert.Single(deserialized);
    }

    [Fact]
    public void Create_WithOccasions_ShouldSerializeCorrectly()
    {
        var profileId = _faker.Random.Guid().ToString();
        var topicKey = "cloud-native";
        var topicName = "Cloud Native";
        var occasion = new Occasion(75, DateTimeOffset.UtcNow.AddDays(-1));

        var topic = ProfileTopic.Create(profileId, topicKey, topicName, true, new[] { occasion });

        Assert.Equal(profileId, topic.ProfileId);
        Assert.Equal(topicKey.ToLowerInvariant(), topic.TopicKey);
        Assert.Equal(topicName, topic.TopicName);
        Assert.True(topic.IsManual);
        Assert.Single(topic.Occasions);
    }

    [Fact]
    public void RoundTrip_ShouldPreserveOccasions()
    {
        var profileId = _faker.Random.Guid().ToString();
        var topicKey = "kubernetes";
        var topicName = "Kubernetes";
        var occasions = new[]
        {
            new Occasion(90, DateTimeOffset.UtcNow.AddDays(-30)),
            new Occasion(70, DateTimeOffset.UtcNow.AddDays(-15)),
            new Occasion(50, DateTimeOffset.UtcNow.AddDays(-1))
        };

        var originalTopic = ProfileTopic.Create(profileId, topicKey, topicName, true, occasions);
        var json = JsonSerializer.Serialize(originalTopic.Occasions, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var roundTripOccasions = JsonSerializer.Deserialize<IReadOnlyCollection<Occasion>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.NotNull(roundTripOccasions);
        Assert.Equal(originalTopic.Occasions.Count, roundTripOccasions.Count);
    }
}
