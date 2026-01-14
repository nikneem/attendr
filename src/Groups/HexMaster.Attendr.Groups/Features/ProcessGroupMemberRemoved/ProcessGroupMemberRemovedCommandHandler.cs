using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.Features.ProcessGroupMemberRemoved;

/// <summary>
/// Command handler to process GroupMemberRemoved events and add ProfileLeftGroup activities to the group.
/// </summary>
public sealed class ProcessGroupMemberRemovedCommandHandler : ICommandHandler<ProcessGroupMemberRemovedCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<ProcessGroupMemberRemovedCommandHandler> _logger;

    public ProcessGroupMemberRemovedCommandHandler(
        IGroupRepository groupRepository,
        ILogger<ProcessGroupMemberRemovedCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(ProcessGroupMemberRemovedCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Groups.StartActivity("ProcessGroupMemberRemoved", ActivityKind.Internal);
        activity?.SetTag("group.id", command.Event.GroupId);
        activity?.SetTag("profile.id", command.Event.ProfileId);

        try
        {
            // Get the group
            var group = await _groupRepository.GetByIdAsync(command.Event.GroupId, cancellationToken);

            if (group == null)
            {
                _logger.LogWarning("Group {GroupId} not found when processing GroupMemberRemoved event",
                    command.Event.GroupId);
                activity?.SetStatus(ActivityStatusCode.Error, "Group not found");
                return;
            }

            activity?.SetTag("group.name", group.Name);

            // Add ProfileLeftGroup activity to the group
            var activityDescription = "left the group";
            group.AddActivity(command.Event.ProfileId, activityDescription, GroupActivityType.ProfileLeftGroup);

            // Persist changes
            await _groupRepository.UpdateAsync(group, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger.LogInformation(
                "Added ProfileLeftGroup activity for profile {ProfileId} in group {GroupId}",
                command.Event.ProfileId, command.Event.GroupId);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _logger.LogError(ex,
                "Failed to process GroupMemberRemoved event for profile {ProfileId} in group {GroupId}",
                command.Event.ProfileId, command.Event.GroupId);
            throw;
        }
    }
}
