using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.DeleteTopic;

/// <summary>
/// Command to delete a topic.
/// When a topic is deleted, all its references to presentations are also deleted.
/// </summary>
/// <param name="Id">The unique identifier of the topic to delete.</param>
public sealed record DeleteTopicCommand(Guid Id) : IAttendrCommand;
