using System.Diagnostics;
using HexMaster.Attendr.Conferences.Integrations.Abstractions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using Microsoft.Extensions.Logging;

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
    private readonly IConferencesIntegrationService _conferencesIntegration;
    private readonly IProfilesIntegrationService _profilesIntegration;
    private readonly ILogger<ProcessProfileCheckedInCommandHandler> _logger;

    public ProcessProfileCheckedInCommandHandler(
        IGroupRepository groupRepository,
        ICheckInRepository checkInRepository,
        IConferencesIntegrationService conferencesIntegration,
        IProfilesIntegrationService profilesIntegration,
        ILogger<ProcessProfileCheckedInCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _checkInRepository = checkInRepository ?? throw new ArgumentNullException(nameof(checkInRepository));
        _conferencesIntegration = conferencesIntegration ?? throw new ArgumentNullException(nameof(conferencesIntegration));
        _profilesIntegration = profilesIntegration ?? throw new ArgumentNullException(nameof(profilesIntegration));
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

            // Fetch profile details from Profiles service
            var profileDetails = await _profilesIntegration.GetProfileDetails(
                command.Event.ProfileId.ToString(), 
                cancellationToken);

            // Create checked-in member from the profile
            var checkedInMember = new CheckedInMember(
                command.Event.ProfileId,
                profileDetails?.DisplayName ?? "Member", // Use actual display name or fallback
                profileDetails?.ProfilePictureUrl); // Use actual profile picture URL

            if (profileDetails == null)
            {
                _logger.LogWarning(
                    "Could not fetch profile details for {ProfileId}, using placeholder values",
                    command.Event.ProfileId);
            }

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
                        // Fetch full presentation details from Conferences service
                        var presentationDto = await _conferencesIntegration.GetPresentationDetails(
                            command.Event.ConferenceId,
                            command.Event.PresentationId,
                            cancellationToken);

                        if (presentationDto == null)
                        {
                            _logger.LogWarning(
                                "Could not fetch presentation details for conference {ConferenceId} and presentation {PresentationId}, using event data",
                                command.Event.ConferenceId,
                                command.Event.PresentationId);

                            // Fallback to event data if presentation not found
                            var fallbackPresentationData = new PresentationData(
                                command.Event.PresentationId,
                                command.Event.Title,
                                string.Empty,
                                command.Event.Room,
                                command.Event.StartDateTime,
                                command.Event.EndDateTime,
                                Array.Empty<PresentationSpeaker>());

                            // Set expiration to 10 minutes after the presentation ends
                            var expiration = command.Event.EndDateTime.AddMinutes(10);

                            var fallbackCheckIn = CheckIn.Create(
                                group.Id,
                                command.Event.ConferenceId,
                                command.Event.PresentationId,
                                fallbackPresentationData,
                                expiration);

                            fallbackCheckIn.AddMember(checkedInMember);
                            await _checkInRepository.AddAsync(fallbackCheckIn, cancellationToken);
                        }
                        else
                        {
                            // Map speakers from DTO to domain model
                            var speakers = presentationDto.Speakers
                                .Select(s => new PresentationSpeaker(s.Id, s.Name, s.ProfilePictureUrl))
                                .ToArray();

                            // Create presentation data from fetched details
                            var presentationData = new PresentationData(
                                presentationDto.Id,
                                presentationDto.Title,
                                presentationDto.Abstract,
                                presentationDto.RoomName,
                                presentationDto.StartDateTime,
                                presentationDto.EndDateTime,
                                speakers);

                            // Set expiration to 10 minutes after the presentation ends (ensure UTC)
                            var expiration = presentationDto.EndDateTime.AddMinutes(10);

                            var newCheckIn = CheckIn.Create(
                                group.Id,
                                command.Event.ConferenceId,
                                command.Event.PresentationId,
                                presentationData,
                                expiration);

                            newCheckIn.AddMember(checkedInMember);
                            await _checkInRepository.AddAsync(newCheckIn, cancellationToken);

                            _logger.LogInformation(
                                "Created check-in {CheckInId} for group {GroupId}, conference {ConferenceId}, presentation {PresentationId} with {SpeakerCount} speakers and added member {ProfileId} ({MemberName})",
                                newCheckIn.Id,
                                group.Id,
                                command.Event.ConferenceId,
                                command.Event.PresentationId,
                                speakers.Length,
                                command.Event.ProfileId,
                                profileDetails?.DisplayName ?? "Unknown");
                        }
                    }
                    else
                    {
                        // Add member to existing check-in
                        await _checkInRepository.AddMemberAsync(existingCheckIn.Id, checkedInMember, cancellationToken);

                        _logger.LogInformation(
                            "Added member {ProfileId} ({MemberName}) to existing check-in {CheckInId} for group {GroupId}",
                            command.Event.ProfileId,
                            profileDetails?.DisplayName ?? "Unknown",
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
                            "Removed member {ProfileId} ({MemberName}) from check-in {CheckInId} for group {GroupId}",
                            command.Event.ProfileId,
                            profileDetails?.DisplayName ?? "Unknown",
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
