using System.Text.Json;
using HexMaster.Attendr.Profiles.Data.TableStorage.Models;
using HexMaster.Attendr.Profiles.DomainModels;

namespace HexMaster.Attendr.Profiles.Data.TableStorage.Mappers;

internal static class ProfileTopicMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static ProfileTopicEntity ToEntity(ProfileTopic topic)
    {
        var occasionsJson = JsonSerializer.Serialize(topic.Occasions, SerializerOptions);

        return new ProfileTopicEntity
        {
            PartitionKey = topic.ProfileId,
            RowKey = topic.TopicKey,
            Id = topic.Id,
            ProfileId = topic.ProfileId,
            TopicKey = topic.TopicKey,
            TopicName = topic.TopicName,
            OccasionsJson = occasionsJson,
            IsManual = topic.IsManual,
            CreatedOn = topic.CreatedOn,
            ModifiedOn = topic.ModifiedOn
        };
    }

    public static ProfileTopic ToDomain(ProfileTopicEntity entity)
    {
        var occasions = DeserializeOccasions(entity.OccasionsJson);

        return ProfileTopic.FromPersisted(
            entity.Id,
            entity.ProfileId,
            entity.TopicKey,
            entity.TopicName,
            entity.IsManual,
            occasions,
            entity.CreatedOn,
            entity.ModifiedOn);
    }

    private static IReadOnlyCollection<Occasion> DeserializeOccasions(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<Occasion>();
        }

        var occasions = JsonSerializer.Deserialize<IReadOnlyCollection<Occasion>>(json, SerializerOptions);
        return occasions ?? Array.Empty<Occasion>();
    }
}
