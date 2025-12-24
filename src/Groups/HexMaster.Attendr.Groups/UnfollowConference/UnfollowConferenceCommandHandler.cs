using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.UnfollowConference;

/// <summary>
/// Command handler to unfollow a conference in a group.
/// Validates that the requesting user is a member of the group.
/// </summary>
public sealed class UnfollowConferenceCommandHandler : ICommandHandler<UnfollowConferenceCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly GroupMetrics _metrics;
    private readonly ILogger<UnfollowConferenceCommandHandler> _logger;

    public UnfollowConferenceCommandHandler(
        IGroupRepository groupRepository,
        GroupMetrics metrics,
        ILogger<UnfollowConferenceCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UnfollowConferenceCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Groups.StartActivity("UnfollowConference", ActivityKind.Internal);
        activity?.SetTag("group.id", command.GroupId);
        activity?.SetTag("conference.id", command.ConferenceId);
        activity?.SetTag("requesting_profile.id", command.RequestingProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get group from repository
            var group = await _groupRepository.GetByIdAsync(command.GroupId, cancellationToken);

            if (group is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Group not found");
                _metrics.RecordOperationFailed("UnfollowConference", "GroupNotFound");
                _metrics.RecordOperationDuration("UnfollowConference", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Attempted to unfollow conference in non-existent group {GroupId}", command.GroupId);
                throw new InvalidOperationException("Group not found.");
            }

            activity?.SetTag("group.name", group.Name);

            // Verify the requesting user is a member
            var requestingMember = group.Members.FirstOrDefault(m => m.Id == command.RequestingProfileId);
            if (requestingMember is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Requesting user is not a member");
                _metrics.RecordOperationFailed("UnfollowConference", "NotAMember");
                _metrics.RecordOperationDuration("UnfollowConference", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} is not a member of group {GroupId}",
                    command.RequestingProfileId, command.GroupId);
                throw new InvalidOperationException("You are not a member of this group.");
            }

            // Unfollow the conference (domain logic will validate if not following)
            group.UnfollowConference(command.ConferenceId);

            // Persist changes
            await _groupRepository.UpdateAsync(group, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("UnfollowConference", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Group {GroupId} is no longer following conference {ConferenceId} (requested by profile {ProfileId})",
                command.GroupId, command.ConferenceId, command.RequestingProfileId);
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
            _metrics.RecordOperationFailed("UnfollowConference", ex.GetType().Name);
            _metrics.RecordOperationDuration("UnfollowConference", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to unfollow conference {ConferenceId} in group {GroupId}",
                command.ConferenceId, command.GroupId);
            throw;
        }
    }
}
