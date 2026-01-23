using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Profiles.Features.SetTopicManualStatus;

/// <summary>
/// Command to set the manual status of a profile topic.
/// </summary>
/// <param name="TopicId">The unique identifier of the topic.</param>
/// <param name="IsManual">True to mark as manual, false to mark as AI-generated.</param>
public sealed record SetTopicManualStatusCommand(
    string TopicId,
    bool IsManual) : IAttendrCommand;
