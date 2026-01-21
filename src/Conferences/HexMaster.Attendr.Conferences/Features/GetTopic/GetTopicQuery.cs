namespace HexMaster.Attendr.Conferences.Features.GetTopic;

/// <summary>
/// Query to retrieve a specific topic by ID.
/// </summary>
/// <param name="TopicId">The unique identifier of the topic.</param>
public sealed record GetTopicQuery(Guid TopicId);
