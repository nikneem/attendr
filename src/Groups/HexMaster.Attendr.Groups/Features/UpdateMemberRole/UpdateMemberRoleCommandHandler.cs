using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using HexMaster.Attendr.Groups.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Groups.Features.UpdateMemberRole;

/// <summary>
/// Command handler to update a member's role in a group.
/// Validates that the requesting user is the group owner.
/// Prevents changing owner role through this endpoint.
/// </summary>
public sealed class UpdateMemberRoleCommandHandler : ICommandHandler<UpdateMemberRoleCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly GroupMetrics _metrics;
    private readonly ILogger<UpdateMemberRoleCommandHandler> _logger;

    public UpdateMemberRoleCommandHandler(
        IGroupRepository groupRepository,
        GroupMetrics metrics,
        ILogger<UpdateMemberRoleCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UpdateMemberRoleCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Groups.StartActivity("UpdateMemberRole", ActivityKind.Internal);
        activity?.SetTag("group.id", command.GroupId);
        activity?.SetTag("member.id", command.MemberId);
        activity?.SetTag("new_role", command.NewRole.ToString());
        activity?.SetTag("requesting_profile.id", command.RequestingProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get group from repository
            var group = await _groupRepository.GetByIdAsync(command.GroupId, cancellationToken);

            if (group is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Group not found");
                _metrics.RecordOperationFailed("UpdateMemberRole", "GroupNotFound");
                _metrics.RecordOperationDuration("UpdateMemberRole", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Attempted to update member role in non-existent group {GroupId}", command.GroupId);
                throw new InvalidOperationException("Group not found.");
            }

            activity?.SetTag("group.name", group.Name);

            // Verify the requesting user is the owner
            var requestingMember = group.Members.FirstOrDefault(m => m.Id == command.RequestingProfileId);
            if (requestingMember is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Requesting user is not a member");
                _metrics.RecordOperationFailed("UpdateMemberRole", "NotAMember");
                _metrics.RecordOperationDuration("UpdateMemberRole", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} is not a member of group {GroupId}",
                    command.RequestingProfileId, command.GroupId);
                throw new InvalidOperationException("You are not a member of this group.");
            }

            // Check if requesting user is the owner
            if (requestingMember.Role != GroupRole.Owner)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Insufficient permissions");
                _metrics.RecordOperationFailed("UpdateMemberRole", "InsufficientPermissions");
                _metrics.RecordOperationDuration("UpdateMemberRole", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} does not have permission to update member roles in group {GroupId}",
                    command.RequestingProfileId, command.GroupId);
                throw new InvalidOperationException("Only group owners can change member roles.");
            }

            // Check if the member exists
            var memberToUpdate = group.Members.FirstOrDefault(m => m.Id == command.MemberId);
            if (memberToUpdate is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Member not found");
                _metrics.RecordOperationFailed("UpdateMemberRole", "MemberNotFound");
                _metrics.RecordOperationDuration("UpdateMemberRole", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Member {MemberId} not found in group {GroupId}",
                    command.MemberId, command.GroupId);
                throw new InvalidOperationException("The specified member is not in this group.");
            }

            // Update the member's role using the domain method (which validates owner changes)
            group.UpdateMemberRole(command.MemberId, command.NewRole);

            // Persist changes
            await _groupRepository.UpdateAsync(group, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("UpdateMemberRole", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Profile {RequestingProfileId} updated role of member {MemberId} to {NewRole} in group {GroupId}",
                command.RequestingProfileId, command.MemberId, command.NewRole, command.GroupId);
        }
        catch (InvalidOperationException)
        {
            // Re-throw domain exceptions
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("UpdateMemberRole", ex.GetType().Name);
            _metrics.RecordOperationDuration("UpdateMemberRole", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to update role of member {MemberId} in group {GroupId}",
                command.MemberId, command.GroupId);
            throw;
        }
    }
}
