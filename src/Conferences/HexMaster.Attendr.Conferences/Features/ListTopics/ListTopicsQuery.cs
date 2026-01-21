namespace HexMaster.Attendr.Conferences.Features.ListTopics;

/// <summary>
/// Query to list topics.
/// </summary>
/// <param name="OnlyVisible">If true, only return visible topics.</param>
public sealed record ListTopicsQuery(bool OnlyVisible = true);
