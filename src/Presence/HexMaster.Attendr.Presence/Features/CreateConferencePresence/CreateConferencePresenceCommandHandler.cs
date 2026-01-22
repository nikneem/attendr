using System.Diagnostics;
using HexMaster.Attendr.Conferences.Integrations.Abstractions;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.CreateConferencePresence;

/// <summary>
/// Command handler to create conference presence records for profiles.
/// Creates presence tracking when users follow a conference.
/// Also creates presentation presence records for all presentations in the conference.
/// </summary>
public sealed class CreateConferencePresenceCommandHandler : ICommandHandler<CreateConferencePresenceCommand>
{
    private readonly IConferencesIntegrationService _conferencesIntegration;
    private readonly IConferencePresenceRepository _repository;
    private readonly IPresentationPresenceRepository _presentationRepository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<CreateConferencePresenceCommandHandler> _logger;

    public CreateConferencePresenceCommandHandler(
        IConferencesIntegrationService conferencesIntegration,
        IConferencePresenceRepository repository,
        IPresentationPresenceRepository presentationRepository,
        PresenceMetrics metrics,
        ILogger<CreateConferencePresenceCommandHandler> logger)
    {
        _conferencesIntegration = conferencesIntegration ?? throw new ArgumentNullException(nameof(conferencesIntegration));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _presentationRepository = presentationRepository ?? throw new ArgumentNullException(nameof(presentationRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(CreateConferencePresenceCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("CreateConferencePresence", ActivityKind.Internal);
        activity?.SetTag("presence.conference_id", command.ConferenceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            ArgumentNullException.ThrowIfNull(command.ProfileIds);

            var profileList = command.ProfileIds.ToList();
            var profileCount = profileList.Count;
            activity?.SetTag("presence.profile_count", profileCount);

            // Fetch conference details once (includes all presentations and speakers)
            var details = await _conferencesIntegration.GetConferenceDetails(command.ConferenceId, cancellationToken);
            if (details is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Conference not found");
                _logger.LogWarning("Conference {ConferenceId} not found", command.ConferenceId);
                throw new InvalidOperationException($"Conference {command.ConferenceId} not found");
            }

            activity?.SetTag("presence.presentation_count", details.Presentations.Count);

            // Create a lookup dictionary for speakers to efficiently find speaker details
            var speakerLookup = details.Speakers.ToDictionary(s => s.Id, s => s);

            // Create presence records for each profile
            foreach (var profileId in profileList)
            {
                var exists = await _repository.ExistsAsync(profileId, command.ConferenceId, cancellationToken);
                if (exists)
                {
                    _logger.LogDebug(
                        "Presence already exists for profile {ProfileId} and conference {ConferenceId}",
                        profileId,
                        command.ConferenceId);
                    continue;
                }

                // Create conference presence
                var presence = new ConferencePresence(
                    command.ConferenceId,
                    details.Title.ToString(),
                    $"{details.City}, {details.Country}",
                    DateOnly.Parse(details.StartDate.ToString()),
                    DateOnly.Parse(details.EndDate.ToString()),
                    profileId,
                    imageUrl: details.ImageUrl,
                    isFollowing: true,
                    isAttending: false,
                    presentations: null);

                await _repository.AddAsync(presence, cancellationToken);

                _logger.LogInformation(
                    "Created conference presence for profile {ProfileId} and conference {ConferenceId}",
                    profileId,
                    command.ConferenceId);

                // Build all presentation presences for this profile in memory
                var presentationPresences = new List<PresentationPresence>(details.Presentations.Count);

                foreach (var presentation in details.Presentations)
                {
                    // Map speaker DTOs to PresentationSpeaker domain objects
                    var speakers = presentation.Speakers
                        .Select(speakerDto =>
                        {
                            // Get full speaker details from lookup
                            if (speakerLookup.TryGetValue(speakerDto.Id, out var fullSpeaker))
                            {
                                return new PresentationSpeaker(
                                    fullSpeaker.Id,
                                    fullSpeaker.Name,
                                    fullSpeaker.ProfilePictureUrl);
                            }
                            // Fallback to DTO data if not found in lookup
                            return new PresentationSpeaker(
                                speakerDto.Id,
                                speakerDto.Name,
                                speakerDto.ProfilePictureUrl);
                        })
                        .ToList();

                    // Map topic DTOs to PresentationTopic domain objects
                    var topics = presentation.Topics
                        .Select(topicDto => new PresentationTopic(topicDto.Key, topicDto.Name))
                        .ToList();

                    var presentationPresence = new PresentationPresence(
                        profileId,
                        command.ConferenceId,
                        presentation.Id,
                        presentation.Title,
                        presentation.Abstract,
                        presentation.RoomName,
                        presentation.StartDateTime,
                        presentation.EndDateTime,
                        speakers,
                        topics,
                        isRated: false,
                        isFavorite: false,
                        isCheckedIn: false,
                        rating: null);

                    presentationPresences.Add(presentationPresence);
                }

                // Bulk insert all presentation presences for this profile in a single operation
                if (presentationPresences.Count > 0)
                {
                    await _presentationRepository.AddManyAsync(presentationPresences, cancellationToken);

                    _logger.LogInformation(
                        "Bulk created {PresentationCount} presentation presences for profile {ProfileId} and conference {ConferenceId}",
                        presentationPresences.Count,
                        profileId,
                        command.ConferenceId);
                }
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordPresenceCreated(profileCount);
            _metrics.RecordOperationDuration("CreateConferencePresence", stopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("CreateConferencePresence", ex.GetType().Name);
            _metrics.RecordOperationDuration("CreateConferencePresence", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
