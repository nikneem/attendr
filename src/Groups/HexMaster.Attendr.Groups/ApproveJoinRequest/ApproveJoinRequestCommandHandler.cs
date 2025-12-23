using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.ApproveJoinRequest;

/// <summary>
/// Command handler to approve a join request.
/// Validates that the requesting user is an owner or manager.
/// Moves the profile from join requests to members.
/// </summary>
public sealed class ApproveJoinRequestCommandHandler : ICommandHandler<ApproveJoinRequestCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly GroupMetrics _metrics;
    private readonly ILogger<ApproveJoinRequestCommandHandler> _logger;

    public ApproveJoinRequestCommandHandler(
        IGroupRepository groupRepository,
        GroupMetrics metrics,
        ILogger<ApproveJoinRequestCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(ApproveJoinRequestCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Groups.StartActivity("ApproveJoinRequest", ActivityKind.Internal);
        activity?.SetTag("group.id", command.GroupId);
        activity?.SetTag("profile_to_approve.id", command.ProfileIdToApprove);
        activity?.SetTag("requesting_profile.id", command.RequestingProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get group from repository
            var group = await _groupRepository.GetByIdAsync(command.GroupId, cancellationToken);

            if (group is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Group not found");
                _metrics.RecordOperationFailed("ApproveJoinRequest", "GroupNotFound");
                _metrics.RecordOperationDuration("ApproveJoinRequest", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Attempted to approve join request for non-existent group {GroupId}", command.GroupId);
                throw new InvalidOperationException("Group not found.");
            }

            activity?.SetTag("group.name", group.Name);

            // Verify the requesting user is a member and has permission
            var requestingMember = group.Members.FirstOrDefault(m => m.Id == command.RequestingProfileId);
            if (requestingMember is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Requesting user is not a member");
                _metrics.RecordOperationFailed("ApproveJoinRequest", "NotAMember");
                _metrics.RecordOperationDuration("ApproveJoinRequest", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} is not a member of group {GroupId}",
                    command.RequestingProfileId, command.GroupId);
                throw new InvalidOperationException("You are not a member of this group.");
            }

            // Check if requesting user is owner or manager
            if (requestingMember.Role != GroupRole.Owner && requestingMember.Role != GroupRole.Manager)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Insufficient permissions");
                _metrics.RecordOperationFailed("ApproveJoinRequest", "InsufficientPermissions");
                _metrics.RecordOperationDuration("ApproveJoinRequest", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} does not have permission to approve join requests for group {GroupId}",
                    command.RequestingProfileId, command.GroupId);
                throw new InvalidOperationException("You do not have permission to approve join requests for this group.");
            }

            // Approve the join request (this will move from join requests to members)
            group.ApproveJoinRequest(command.ProfileIdToApprove);

            // Persist changes
            await _groupRepository.UpdateAsync(group, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("ApproveJoinRequest", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Profile {RequestingProfileId} approved join request for {ProfileId} in group {GroupId}",
                command.RequestingProfileId, command.ProfileIdToApprove, command.GroupId);
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
            _metrics.RecordOperationFailed("ApproveJoinRequest", ex.GetType().Name);
            _metrics.RecordOperationDuration("ApproveJoinRequest", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to approve join request for {ProfileId} in group {GroupId}",
                command.ProfileIdToApprove, command.GroupId);
            throw;
        }
    }
}
