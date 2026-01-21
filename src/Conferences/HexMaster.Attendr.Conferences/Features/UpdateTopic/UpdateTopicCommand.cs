using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.UpdateTopic;

/// <summary>
/// Command to update an existing topic.
/// Topic ID cannot be changed. Only key, name, and visibility can be updated.
/// </summary>
/// <param name="Id">The unique identifier of the topic (cannot be changed).</param>
/// <param name="Key">The updated unique key of the topic.</param>
/// <param name="Name">The updated display name of the topic.</param>
/// <param name="IsVisible">Whether the topic should be visible to users.</param>
public sealed record UpdateTopicCommand(
    Guid Id,
    string Key,
    string Name,
    bool IsVisible) : IAttendrCommand;
