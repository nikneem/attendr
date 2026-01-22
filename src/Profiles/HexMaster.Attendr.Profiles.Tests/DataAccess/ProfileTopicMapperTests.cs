using Bogus;
using HexMaster.Attendr.Profiles.Data.TableStorage.Mappers;
using HexMaster.Attendr.Profiles.Data.TableStorage.Models;
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

    [Fact]
    public void ToEntity_ShouldMapAndSerializeOccasions()
    {
        var profileId = _faker.Random.Guid().ToString();
        var topic = ProfileTopic.Create(
            profileId,
            "ai-ml",
            "AI & ML",
            true,
            new[] { new Occasion(80, DateTimeOffset.UtcNow.AddDays(-3)) });

        var entity = ProfileTopicMapper.ToEntity(topic);

        Assert.Equal(profileId, entity.PartitionKey);
        Assert.Equal(topic.TopicKey, entity.RowKey);
        Assert.Equal(topic.TopicName, entity.TopicName);
        Assert.True(entity.IsManual);

        var occasions = JsonSerializer.Deserialize<IReadOnlyCollection<Occasion>>(entity.OccasionsJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(occasions);
        Assert.Single(occasions);
    }

    [Fact]
    public void ToDomain_ShouldHandleEmptyOccasionsJson()
    {
        var entity = new ProfileTopicEntity
        {
            Id = _faker.Random.Guid().ToString(),
            ProfileId = _faker.Random.Guid().ToString(),
            TopicKey = "observability",
            TopicName = "Observability",
            OccasionsJson = string.Empty,
            IsManual = false,
            CreatedOn = DateTimeOffset.UtcNow.AddDays(-10),
            ModifiedOn = null
        };

        var domain = ProfileTopicMapper.ToDomain(entity);

        Assert.Equal(entity.ProfileId, domain.ProfileId);
        Assert.Equal(entity.TopicKey, domain.TopicKey);
        Assert.Empty(domain.Occasions);
    }

    [Fact]
    public void Mapper_RoundTrip_ShouldPreserveValues()
    {
        var profileId = _faker.Random.Guid().ToString();
        var occasions = new[]
        {
            new Occasion(100, DateTimeOffset.UtcNow.AddDays(-7)),
            new Occasion(60, DateTimeOffset.UtcNow.AddDays(-2))
        };

        var original = ProfileTopic.Create(profileId, "devrel", "Developer Relations", false, occasions);

        var entity = ProfileTopicMapper.ToEntity(original);
        var mappedBack = ProfileTopicMapper.ToDomain(entity);

        Assert.Equal(original.ProfileId, mappedBack.ProfileId);
        Assert.Equal(original.TopicKey, mappedBack.TopicKey);
        Assert.Equal(original.TopicName, mappedBack.TopicName);
        Assert.Equal(original.IsManual, mappedBack.IsManual);
        Assert.Equal(original.Occasions.Count, mappedBack.Occasions.Count);
    }
}
