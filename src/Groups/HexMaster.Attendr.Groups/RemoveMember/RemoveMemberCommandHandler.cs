using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.RemoveMember;

/// <summary>
/// Command handler to remove a member from a group.
/// Validates that the requesting user is an owner or manager.
/// Prevents removal of the last owner.
/// </summary>
public sealed class RemoveMemberCommandHandler : ICommandHandler<RemoveMemberCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly GroupMetrics _metrics;
    private readonly ILogger<RemoveMemberCommandHandler> _logger;

    public RemoveMemberCommandHandler(
        IGroupRepository groupRepository,
        GroupMetrics metrics,
        ILogger<RemoveMemberCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(RemoveMemberCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Groups.StartActivity("RemoveMember", ActivityKind.Internal);
        activity?.SetTag("group.id", command.GroupId);
        activity?.SetTag("member_to_remove.id", command.MemberIdToRemove);
        activity?.SetTag("requesting_profile.id", command.RequestingProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get group from repository
            var group = await _groupRepository.GetByIdAsync(command.GroupId, cancellationToken);

            if (group is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Group not found");
                _metrics.RecordOperationFailed("RemoveMember", "GroupNotFound");
                _metrics.RecordOperationDuration("RemoveMember", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Attempted to remove member from non-existent group {GroupId}", command.GroupId);
                throw new InvalidOperationException("Group not found.");
            }

            activity?.SetTag("group.name", group.Name);

            // Verify the requesting user is a member and has permission
            var requestingMember = group.Members.FirstOrDefault(m => m.Id == command.RequestingProfileId);
            if (requestingMember is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Requesting user is not a member");
                _metrics.RecordOperationFailed("RemoveMember", "NotAMember");
                _metrics.RecordOperationDuration("RemoveMember", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} is not a member of group {GroupId}",
                    command.RequestingProfileId, command.GroupId);
                throw new InvalidOperationException("You are not a member of this group.");
            }

            // Check if requesting user is owner or manager
            if (requestingMember.Role != GroupRole.Owner && requestingMember.Role != GroupRole.Manager)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Insufficient permissions");
                _metrics.RecordOperationFailed("RemoveMember", "InsufficientPermissions");
                _metrics.RecordOperationDuration("RemoveMember", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} does not have permission to remove members from group {GroupId}",
                    command.RequestingProfileId, command.GroupId);
                throw new InvalidOperationException("You do not have permission to remove members from this group.");
            }

            // Check if the member to remove exists
            var memberToRemove = group.Members.FirstOrDefault(m => m.Id == command.MemberIdToRemove);
            if (memberToRemove is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Member to remove not found");
                _metrics.RecordOperationFailed("RemoveMember", "MemberNotFound");
                _metrics.RecordOperationDuration("RemoveMember", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Member {MemberId} not found in group {GroupId}",
                    command.MemberIdToRemove, command.GroupId);
                throw new InvalidOperationException("The specified member is not in this group.");
            }

            // Prevent removal of the last owner
            if (memberToRemove.Role == GroupRole.Owner)
            {
                var ownerCount = group.Members.Count(m => m.Role == GroupRole.Owner);
                if (ownerCount <= 1)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Cannot remove last owner");
                    _metrics.RecordOperationFailed("RemoveMember", "LastOwner");
                    _metrics.RecordOperationDuration("RemoveMember", stopwatch.Elapsed.TotalMilliseconds, success: false);

                    _logger.LogWarning("Attempted to remove the last owner from group {GroupId}", command.GroupId);
                    throw new InvalidOperationException("Cannot remove the last owner of the group. Transfer ownership first.");
                }
            }

            // Remove the member
            group.RemoveMember(command.MemberIdToRemove);

            // Persist changes
            await _groupRepository.UpdateAsync(group, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("RemoveMember", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Profile {RequestingProfileId} removed member {MemberId} from group {GroupId}",
                command.RequestingProfileId, command.MemberIdToRemove, command.GroupId);
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
            _metrics.RecordOperationFailed("RemoveMember", ex.GetType().Name);
            _metrics.RecordOperationDuration("RemoveMember", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to remove member {MemberId} from group {GroupId}",
                command.MemberIdToRemove, command.GroupId);
            throw;
        }
    }
}
