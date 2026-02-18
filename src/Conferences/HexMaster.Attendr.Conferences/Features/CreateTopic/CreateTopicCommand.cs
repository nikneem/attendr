using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.CreateTopic;

/// <summary>
/// Command to create a new topic.
/// </summary>
/// <param name="Key">The unique key of the topic.</param>
/// <param name="Name">The display name of the topic.</param>
/// <param name="IsManual">When true, the topic was created manually and should be visible by default.</param>
public sealed record CreateTopicCommand(
    string Key,
    string Name,
    bool IsManual = false) : IAttendrCommand;
