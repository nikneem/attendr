using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.Features.ProcessProfileCheckedIn;

/// <summary>
/// Command handler to process ProfileCheckedIn events and add activities to groups.
/// When a profile checks in or out of a presentation, all groups where the profile
/// is an active member will have an activity added.
/// </summary>
public sealed class ProcessProfileCheckedInCommandHandler : ICommandHandler<ProcessProfileCheckedInCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<ProcessProfileCheckedInCommandHandler> _logger;

    public ProcessProfileCheckedInCommandHandler(
        IGroupRepository groupRepository,
        ILogger<ProcessProfileCheckedInCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(ProcessProfileCheckedInCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Groups.StartActivity("ProcessProfileCheckedIn", ActivityKind.Internal);
        activity?.SetTag("profile.id", command.Event.ProfileId);
        activity?.SetTag("conference.id", command.Event.ConferenceId);
        activity?.SetTag("presentation.id", command.Event.PresentationId);
        activity?.SetTag("is_checked_in", command.Event.IsCheckedIn);

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

            var activityType = command.Event.IsCheckedIn
                ? GroupActivityType.ProfilePresentationCheckedIn
                : GroupActivityType.ProfilePresentationCheckedOut;

            var activityDescription = command.Event.IsCheckedIn
                ? $"checked in to {command.Event.Title} at {command.Event.Room}"
                : $"checked out of {command.Event.Title} at {command.Event.Room}";

            // Add activity to each group
            foreach (var group in groups)
            {
                group.AddActivity(command.Event.ProfileId, activityDescription, activityType);
                await _groupRepository.UpdateAsync(group, cancellationToken);

                _logger.LogInformation(
                    "Added {ActivityType} activity to group {GroupId} for profile {ProfileId} and presentation {PresentationId}",
                    command.Event.IsCheckedIn ? "check-in" : "check-out",
                    group.Id,
                    command.Event.ProfileId,
                    command.Event.PresentationId);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Processed ProfileCheckedIn event for profile {ProfileId} and updated {GroupCount} groups",
                command.Event.ProfileId,
                groups.Count);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);

            _logger.LogError(ex,
                "Failed to process ProfileCheckedIn event for profile {ProfileId}, presentation {PresentationId}",
                command.Event.ProfileId,
                command.Event.PresentationId);
            throw;
        }
    }
}
