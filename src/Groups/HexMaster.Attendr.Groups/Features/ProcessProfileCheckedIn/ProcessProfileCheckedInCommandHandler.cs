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
    private readonly ICheckInRepository _checkInRepository;
    private readonly ILogger<ProcessProfileCheckedInCommandHandler> _logger;

    public ProcessProfileCheckedInCommandHandler(
        IGroupRepository groupRepository,
        ICheckInRepository checkInRepository,
        ILogger<ProcessProfileCheckedInCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _checkInRepository = checkInRepository ?? throw new ArgumentNullException(nameof(checkInRepository));
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

            // TODO: Fetch profile details (name, picture) from IProfilesIntegrationService
            // Currently using placeholder values as the event doesn't include this data
            // and there's no GetProfileById method available yet
            // Create checked-in member from the profile
            var checkedInMember = new CheckedInMember(
                command.Event.ProfileId,
                "Member", // Placeholder - should be actual profile name
                null); // ProfilePictureUrl not available in event

            // Process each group
            foreach (var group in groups)
            {
                // Add activity to the group
                group.AddActivity(command.Event.ProfileId, activityDescription, activityType);
                await _groupRepository.UpdateAsync(group, cancellationToken);

                if (command.Event.IsCheckedIn)
                {
                    // Check if there's already a check-in for this group, conference, and presentation
                    var existingCheckIn = await _checkInRepository.GetByGroupConferenceAndPresentationAsync(
                        group.Id,
                        command.Event.ConferenceId,
                        command.Event.PresentationId,
                        cancellationToken);

                    if (existingCheckIn == null)
                    {
                        // TODO: Fetch full presentation data (abstract, speakers) from Conferences service
                        // Currently using data available in the event with placeholders for missing fields
                        // Create new check-in with presentation data from the event
                        var presentationData = new PresentationData(
                            command.Event.PresentationId,
                            command.Event.Title,
                            string.Empty, // Abstract not available in event
                            command.Event.Room,
                            command.Event.StartDateTime,
                            command.Event.EndDateTime,
                            Array.Empty<PresentationSpeaker>()); // Speakers not available in event

                        // Set expiration to 2 hours after the presentation ends
                        var expiration = command.Event.EndDateTime.AddMinutes(10);

                        var newCheckIn = CheckIn.Create(
                            group.Id,
                            command.Event.ConferenceId,
                            command.Event.PresentationId,
                            presentationData,
                            expiration);

                        newCheckIn.AddMember(checkedInMember);
                        await _checkInRepository.AddAsync(newCheckIn, cancellationToken);

                        _logger.LogInformation(
                            "Created check-in {CheckInId} for group {GroupId}, conference {ConferenceId}, presentation {PresentationId} and added member {ProfileId}",
                            newCheckIn.Id,
                            group.Id,
                            command.Event.ConferenceId,
                            command.Event.PresentationId,
                            command.Event.ProfileId);
                    }
                    else
                    {
                        // Add member to existing check-in
                        await _checkInRepository.AddMemberAsync(existingCheckIn.Id, checkedInMember, cancellationToken);

                        _logger.LogInformation(
                            "Added member {ProfileId} to existing check-in {CheckInId} for group {GroupId}",
                            command.Event.ProfileId,
                            existingCheckIn.Id,
                            group.Id);
                    }
                }
                else
                {
                    // Handle check-out: remove member from check-in
                    var existingCheckIn = await _checkInRepository.GetByGroupConferenceAndPresentationAsync(
                        group.Id,
                        command.Event.ConferenceId,
                        command.Event.PresentationId,
                        cancellationToken);

                    if (existingCheckIn != null)
                    {
                        await _checkInRepository.RemoveMemberAsync(existingCheckIn.Id, command.Event.ProfileId, cancellationToken);

                        _logger.LogInformation(
                            "Removed member {ProfileId} from check-in {CheckInId} for group {GroupId}",
                            command.Event.ProfileId,
                            existingCheckIn.Id,
                            group.Id);
                    }
                }

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
