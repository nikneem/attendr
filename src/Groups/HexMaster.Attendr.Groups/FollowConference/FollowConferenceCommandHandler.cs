using System.Diagnostics;
using HexMaster.Attendr.Conferences;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.FollowConference;

/// <summary>
/// Command handler to follow a conference in a group.
/// Validates that the requesting user is a member of the group.
/// Validates that the conference exists.
/// </summary>
public sealed class FollowConferenceCommandHandler : ICommandHandler<FollowConferenceCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IConferenceRepository _conferenceRepository;
    private readonly GroupMetrics _metrics;
    private readonly ILogger<FollowConferenceCommandHandler> _logger;

    public FollowConferenceCommandHandler(
        IGroupRepository groupRepository,
        IConferenceRepository conferenceRepository,
        GroupMetrics metrics,
        ILogger<FollowConferenceCommandHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(FollowConferenceCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Groups.StartActivity("FollowConference", ActivityKind.Internal);
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
                _metrics.RecordOperationFailed("FollowConference", "GroupNotFound");
                _metrics.RecordOperationDuration("FollowConference", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Attempted to follow conference in non-existent group {GroupId}", command.GroupId);
                throw new InvalidOperationException("Group not found.");
            }

            activity?.SetTag("group.name", group.Name);

            // Verify the requesting user is a member
            var requestingMember = group.Members.FirstOrDefault(m => m.Id == command.RequestingProfileId);
            if (requestingMember is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Requesting user is not a member");
                _metrics.RecordOperationFailed("FollowConference", "NotAMember");
                _metrics.RecordOperationDuration("FollowConference", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} is not a member of group {GroupId}",
                    command.RequestingProfileId, command.GroupId);
                throw new InvalidOperationException("You are not a member of this group.");
            }

            // Get conference from repository to validate it exists and get details
            var conference = await _conferenceRepository.GetByIdAsync(command.ConferenceId, cancellationToken);

            if (conference is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Conference not found");
                _metrics.RecordOperationFailed("FollowConference", "ConferenceNotFound");
                _metrics.RecordOperationDuration("FollowConference", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Attempted to follow non-existent conference {ConferenceId}", command.ConferenceId);
                throw new InvalidOperationException("Conference not found.");
            }

            activity?.SetTag("conference.title", conference.Title);

            // Follow the conference (domain logic will validate if already following)
            group.FollowConference(
                conference.Id,
                conference.Title,
                conference.City,
                conference.Country,
                conference.StartDate,
                conference.EndDate);

            // Persist changes
            await _groupRepository.UpdateAsync(group, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("FollowConference", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Group {GroupId} is now following conference {ConferenceId} (requested by profile {ProfileId})",
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
            _metrics.RecordOperationFailed("FollowConference", ex.GetType().Name);
            _metrics.RecordOperationDuration("FollowConference", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to follow conference {ConferenceId} in group {GroupId}",
                command.ConferenceId, command.GroupId);
            throw;
        }
    }
}
