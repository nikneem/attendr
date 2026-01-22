namespace HexMaster.Attendr.Profiles.Abstractions.Dtos;

public sealed record ProfileTopicDto(
    string Id,
    string ProfileId,
    string TopicKey,
    string TopicName,
    bool IsManual,
    DateTimeOffset CreatedOn,
    int Weight);
