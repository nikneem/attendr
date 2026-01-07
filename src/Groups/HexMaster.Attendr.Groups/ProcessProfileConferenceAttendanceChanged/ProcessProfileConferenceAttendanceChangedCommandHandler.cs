using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.DomainModels;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.ProcessProfileConferenceAttendanceChanged;

/// <summary>
/// Command handler to process ProfileConferenceAttendanceChanged events and add activities to groups.
/// When a profile changes their conference attendance status, all groups where the profile
/// is an active member will have an activity added.
/// </summary>
public sealed class ProcessProfileConferenceAttendanceChangedCommandHandler : ICommandHandler<ProcessProfileConferenceAttendanceChangedCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<ProcessProfileConferenceAttendanceChangedCommandHandler> _logger;

    public ProcessProfileConferenceAttendanceChangedCommandHandler(
        IGroupRepository groupRepository,
        ILogger<ProcessProfileConferenceAttendanceChangedCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(ProcessProfileConferenceAttendanceChangedCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Groups.StartActivity("ProcessProfileConferenceAttendanceChanged", ActivityKind.Internal);
        activity?.SetTag("profile.id", command.Event.ProfileId);
        activity?.SetTag("conference.id", command.Event.ConferenceId);
        activity?.SetTag("is_attending", command.Event.IsAttending);

        try
        {
            // Find all groups where the profile is an active member
            var groups = await _groupRepository.GetGroupsByMemberIdAsync(command.Event.ProfileId, cancellationToken);

            if (!groups.Any())
            {
                _logger.LogInformation("Profile {ProfileId} is not a member of any groups", command.Event.ProfileId);
                return;
            }

            activity?.SetTag("groups.count", groups.Count);

            var activityType = command.Event.IsAttending
                ? GroupActivityType.ProfileAttendingConference
                : GroupActivityType.ProfileLeavingConference;

            var activityDescription = command.Event.IsAttending
                ? $"is attending {command.Event.ConferenceName}"
                : $"is no longer attending {command.Event.ConferenceName}";

            // Add activity to each group
            foreach (var group in groups)
            {
                group.AddActivity(command.Event.ProfileId, activityDescription, activityType);
                await _groupRepository.UpdateAsync(group, cancellationToken);

                _logger.LogInformation(
                    "Added {ActivityType} activity to group {GroupId} for profile {ProfileId} and conference {ConferenceId}",
                    command.Event.IsAttending ? "attending" : "leaving",
                    group.Id,
                    command.Event.ProfileId,
                    command.Event.ConferenceId);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Processed ProfileConferenceAttendanceChanged event for profile {ProfileId} and updated {GroupCount} groups",
                command.Event.ProfileId,
                groups.Count);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);

            _logger.LogError(ex,
                "Failed to process ProfileConferenceAttendanceChanged event for profile {ProfileId}, conference {ConferenceId}",
                command.Event.ProfileId,
                command.Event.ConferenceId);
            throw;
        }
    }
}
