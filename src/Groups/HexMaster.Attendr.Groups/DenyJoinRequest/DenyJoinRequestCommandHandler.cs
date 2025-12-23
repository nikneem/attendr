using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.DenyJoinRequest;

/// <summary>
/// Command handler to deny a join request.
/// Validates that the requesting user is an owner or manager.
/// Removes the join request from the group.
/// </summary>
public sealed class DenyJoinRequestCommandHandler : ICommandHandler<DenyJoinRequestCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly GroupMetrics _metrics;
    private readonly ILogger<DenyJoinRequestCommandHandler> _logger;

    public DenyJoinRequestCommandHandler(
        IGroupRepository groupRepository,
        GroupMetrics metrics,
        ILogger<DenyJoinRequestCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DenyJoinRequestCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Groups.StartActivity("DenyJoinRequest", ActivityKind.Internal);
        activity?.SetTag("group.id", command.GroupId);
        activity?.SetTag("profile_to_deny.id", command.ProfileIdToDeny);
        activity?.SetTag("requesting_profile.id", command.RequestingProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get group from repository
            var group = await _groupRepository.GetByIdAsync(command.GroupId, cancellationToken);

            if (group is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Group not found");
                _metrics.RecordOperationFailed("DenyJoinRequest", "GroupNotFound");
                _metrics.RecordOperationDuration("DenyJoinRequest", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Attempted to deny join request for non-existent group {GroupId}", command.GroupId);
                throw new InvalidOperationException("Group not found.");
            }

            activity?.SetTag("group.name", group.Name);

            // Verify the requesting user is a member and has permission
            var requestingMember = group.Members.FirstOrDefault(m => m.Id == command.RequestingProfileId);
            if (requestingMember is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Requesting user is not a member");
                _metrics.RecordOperationFailed("DenyJoinRequest", "NotAMember");
                _metrics.RecordOperationDuration("DenyJoinRequest", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} is not a member of group {GroupId}",
                    command.RequestingProfileId, command.GroupId);
                throw new InvalidOperationException("You are not a member of this group.");
            }

            // Check if requesting user is owner or manager
            if (requestingMember.Role != GroupRole.Owner && requestingMember.Role != GroupRole.Manager)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Insufficient permissions");
                _metrics.RecordOperationFailed("DenyJoinRequest", "InsufficientPermissions");
                _metrics.RecordOperationDuration("DenyJoinRequest", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} does not have permission to deny join requests for group {GroupId}",
                    command.RequestingProfileId, command.GroupId);
                throw new InvalidOperationException("You do not have permission to deny join requests for this group.");
            }

            // Deny the join request (this will remove it from the list)
            group.DeclineJoinRequest(command.ProfileIdToDeny);

            // Persist changes
            await _groupRepository.UpdateAsync(group, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("DenyJoinRequest", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Profile {RequestingProfileId} denied join request for {ProfileId} in group {GroupId}",
                command.RequestingProfileId, command.ProfileIdToDeny, command.GroupId);
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
            _metrics.RecordOperationFailed("DenyJoinRequest", ex.GetType().Name);
            _metrics.RecordOperationDuration("DenyJoinRequest", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to deny join request for {ProfileId} in group {GroupId}",
                command.ProfileIdToDeny, command.GroupId);
            throw;
        }
    }
}
