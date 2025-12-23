using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.JoinGroup;

/// <summary>
/// Command handler to process a request to join a group.
/// For public groups, the member is added immediately.
/// For private groups, a join request is created for approval.
/// </summary>
public sealed class JoinGroupCommandHandler : ICommandHandler<JoinGroupCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly GroupMetrics _metrics;
    private readonly ILogger<JoinGroupCommandHandler> _logger;

    public JoinGroupCommandHandler(
        IGroupRepository groupRepository,
        GroupMetrics metrics,
        ILogger<JoinGroupCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(JoinGroupCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Groups.StartActivity("JoinGroup", ActivityKind.Internal);
        activity?.SetTag("group.id", command.GroupId);
        activity?.SetTag("profile.id", command.ProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get group from repository
            var group = await _groupRepository.GetByIdAsync(command.GroupId, cancellationToken);

            if (group is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Group not found");
                _metrics.RecordOperationFailed("JoinGroup", "GroupNotFound");
                _metrics.RecordOperationDuration("JoinGroup", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Attempted to join non-existent group {GroupId}", command.GroupId);
                throw new InvalidOperationException("Group not found.");
            }

            activity?.SetTag("group.name", group.Name);
            activity?.SetTag("group.is_public", group.Settings.IsPublic);

            // Check if already a member
            if (group.Members.Any(m => m.Id == command.ProfileId))
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Already a member");
                _metrics.RecordOperationFailed("JoinGroup", "AlreadyMember");
                _metrics.RecordOperationDuration("JoinGroup", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} is already a member of group {GroupId}",
                    command.ProfileId, command.GroupId);
                throw new InvalidOperationException("You are already a member of this group.");
            }

            if (group.Settings.IsPublic)
            {
                // Public group - add member directly
                group.AddMember(command.ProfileId, command.ProfileName, GroupRole.Member);
                activity?.SetTag("action", "member_added");
                _logger.LogInformation("Profile {ProfileId} joined public group {GroupId} as member",
                    command.ProfileId, command.GroupId);
            }
            else
            {
                // Private group - create join request
                group.AddJoinRequest(command.ProfileId, command.ProfileName);
                activity?.SetTag("action", "join_request_created");
                _logger.LogInformation("Profile {ProfileId} created join request for private group {GroupId}",
                    command.ProfileId, command.GroupId);
            }

            // Persist changes
            await _groupRepository.UpdateAsync(group, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("JoinGroup", stopwatch.Elapsed.TotalMilliseconds, success: true);
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
            _metrics.RecordOperationFailed("JoinGroup", ex.GetType().Name);
            _metrics.RecordOperationDuration("JoinGroup", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to process join request for group {GroupId}", command.GroupId);
            throw;
        }
    }
}
