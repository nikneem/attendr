namespace HexMaster.Attendr.Conferences.DomainModels;

/// <summary>
/// Value object representing a topic associated with a presentation.
/// </summary>
/// <param name="Key">The unique key identifier for the topic.</param>
/// <param name="Name">The display name of the topic.</param>
public sealed record PresentationTopic(string Key, string Name);
