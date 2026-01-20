using HexMaster.Attendr.Conferences.Abstractions.Services;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.DomainModels;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Sessionize.Api.Client.Abstractions;

namespace HexMaster.Attendr.Conferences.Services;

/// <summary>
/// Service for synchronizing conference data from Sessionize.
/// </summary>
public sealed class SessionizeSyncService : ISessionizeSyncService
{
    private readonly IConferenceRepository _conferenceRepository;
    private readonly ISessionizeApiClient _sessionizeApiClient;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<SessionizeSyncService> _logger;

    public SessionizeSyncService(
        IConferenceRepository conferenceRepository,
        ISessionizeApiClient sessionizeApiClient,
        IIntegrationEventPublisher eventPublisher,
        ILogger<SessionizeSyncService> logger)
    {
        _conferenceRepository = conferenceRepository;
        _sessionizeApiClient = sessionizeApiClient;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SessionizeSyncResult?> SynchronizeConferenceAsync(Guid conferenceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Sessionize synchronization for conference {ConferenceId}", conferenceId);

        // Retrieve the conference from the repository
        var conference = await _conferenceRepository.GetByIdAsync(conferenceId, cancellationToken);
        if (conference == null)
        {
            _logger.LogWarning("Conference {ConferenceId} not found", conferenceId);
            return null;
        }

        // Check if the conference has a Sessionize synchronization source
        if (conference.SynchronizationSource == null ||
            conference.SynchronizationSource.SourceType != SynchronizationSourceType.Sessionize ||
            string.IsNullOrWhiteSpace(conference.SynchronizationSource.SourceLocationOrApiKey))
        {
            _logger.LogInformation("Conference {ConferenceId} does not have Sessionize synchronization configured, skipping", conferenceId);
            return new SessionizeSyncResult(
                conference.Id,
                conference.Speakers.Count,
                conference.Rooms.Count,
                conference.Presentations.Count);
        }

        _logger.LogInformation("Fetching data from Sessionize for conference {ConferenceId} with API ID {ApiId}",
            conferenceId, conference.SynchronizationSource.SourceLocationOrApiKey);

        // Fetch speakers
        var sessionizeSpeakers = await _sessionizeApiClient.GetSpeakersListAsync(conference.SynchronizationSource.SourceLocationOrApiKey, cancellationToken);
        var speakerIdMapping = new Dictionary<string, Guid>(); // Maps external ID to local GUID

        foreach (var sessionizeSpeaker in sessionizeSpeakers)
        {
            var externalId = sessionizeSpeaker.Id.ToString();

            // Check if speaker already exists by ExternalId
            var existingSpeaker = conference.Speakers.FirstOrDefault(s => s.ExternalId == externalId);

            if (existingSpeaker != null)
            {
                var speaker = Speaker.FromPersisted(existingSpeaker.Id, existingSpeaker.Name, existingSpeaker.Company,
                    existingSpeaker.ProfilePictureUrl, existingSpeaker.ExternalId);

                speaker.SetName(sessionizeSpeaker.FullName ?? "Unknown");
                speaker.SetCompany(sessionizeSpeaker.TagLine);
                speaker.SetProfilePictureUrl(sessionizeSpeaker.ProfilePicture);
                conference.UpdateSpeaker(speaker);

                _logger.LogDebug("Speaker with ExternalId {ExternalId} already exists as {SpeakerId}", externalId, existingSpeaker.Id);
            }
            else
            {
                var newSpeaker = Speaker.Create(sessionizeSpeaker.FullName ?? "Unknown",
                    sessionizeSpeaker.TagLine,
                    sessionizeSpeaker.ProfilePicture,
                    externalId);

                try
                {
                    conference.AddSpeaker(newSpeaker);
                    speakerIdMapping[externalId] = newSpeaker.Id;
                    _logger.LogDebug("Added speaker {SpeakerId} (ExternalId: {ExternalId}) - {SpeakerName}", newSpeaker.Id, externalId, newSpeaker.Name);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Failed to add speaker {SpeakerId}, skipping", newSpeaker.Id);
                }
            }
        }

        // Fetch schedule to get rooms and sessions
        var scheduleGrid = await _sessionizeApiClient.GetScheduleGridAsync(conference.SynchronizationSource.SourceLocationOrApiKey, cancellationToken);
        var roomIdMapping = new Dictionary<string, Guid>(); // Maps room ID (as external ID) to local GUID

        // First pass: collect all unique rooms and update/add them
        foreach (var day in scheduleGrid)
        {
            foreach (var room in day.Rooms)
            {
                var roomId = room.Id.ToString();

                // Check if room already exists by ExternalId
                var existingRoom = conference.Rooms.FirstOrDefault(r => r.ExternalId == roomId);

                if (existingRoom != null)
                {
                    // Update existing room with new data
                    var updatedRoom = Room.FromPersisted(existingRoom.Id, existingRoom.Name, existingRoom.Capacity, existingRoom.ExternalId);
                    updatedRoom.SetName(room.Name);
                    updatedRoom.SetCapacity(100); // Default capacity from Sessionize doesn't provide this, so keep existing or use default
                    conference.UpdateRoom(updatedRoom);

                    roomIdMapping[roomId] = existingRoom.Id;
                    _logger.LogDebug("Updated room {RoomId} (ExternalId: {ExternalId}) - {RoomName}", existingRoom.Id, roomId, room.Name);
                }
                else if (!roomIdMapping.ContainsKey(roomId))
                {
                    // Create new room
                    var newRoom = Room.Create(room.Name, 100, roomId); // Default capacity, using ID as externalId

                    try
                    {
                        conference.AddRoom(newRoom);
                        roomIdMapping[roomId] = newRoom.Id;
                        _logger.LogDebug("Added room {RoomId} (ExternalId: {ExternalId}) - {RoomName}", newRoom.Id, roomId, newRoom.Name);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.LogWarning(ex, "Failed to add room {RoomName}, skipping", room.Name);
                    }
                }
            }
        }

        // Second pass: add/update presentations
        var updatedPresentations = new List<(Presentation Presentation, bool IsScheduleChanged)>();

        foreach (var day in scheduleGrid)
        {
            foreach (var room in day.Rooms)
            {
                foreach (var session in room.Sessions)
                {
                    if (string.IsNullOrWhiteSpace(session.Id)) continue;

                    // Find the room ID
                    if (!roomIdMapping.TryGetValue(room.Id.ToString(), out var roomLocalId))
                    {
                        _logger.LogWarning("Room {RoomName} not found for session {SessionId}, skipping", room.Name, session.Id);
                        continue;
                    }

                    // Map speaker external IDs to local GUIDs
                    var speakerIds = session.Speakers?
                        .Where(s => !string.IsNullOrWhiteSpace(s.Id))
                        .Select(s => speakerIdMapping.TryGetValue(s.Id, out var localId) ? localId : Guid.Empty)
                        .Where(id => id != Guid.Empty)
                        .ToList() ?? new List<Guid>();

                    if (speakerIds.Count == 0)
                    {
                        _logger.LogWarning("Session {SessionId} has no valid speakers, skipping", session.Id);
                        continue;
                    }

                    // Use the datetime directly from the session
                    var startDateTime = session.StartsAt.DateTime;
                    var endDateTime = session.EndsAt.DateTime;
                    var title = session.Title ?? "Untitled";
                    var description = session.Description ?? "No description available";

                    // Check if presentation already exists by ExternalId
                    var existingPresentation = conference.Presentations
                        .FirstOrDefault(p => p.ExternalId == session.Id);

                    if (existingPresentation != null)
                    {
                        // Track if schedule changed (time or room)
                        var isScheduleChanged =
                            existingPresentation.StartDateTime != startDateTime ||
                            existingPresentation.EndDateTime != endDateTime ||
                            existingPresentation.RoomId != roomLocalId;

                        // Update existing presentation
                        existingPresentation.UpdateDetails(title, description, startDateTime, endDateTime);
                        existingPresentation.ChangeRoom(roomLocalId);

                        // Check for speaker changes
                        var currentSpeakerIds = existingPresentation.SpeakerIds.OrderBy(x => x).ToList();
                        var newSpeakerIds = speakerIds.OrderBy(x => x).ToList();
                        if (!currentSpeakerIds.SequenceEqual(newSpeakerIds))
                        {
                            // Remove speakers not in new list
                            foreach (var speakerId in currentSpeakerIds.Where(s => !newSpeakerIds.Contains(s)))
                            {
                                existingPresentation.RemoveSpeaker(speakerId);
                            }
                            // Add new speakers
                            foreach (var speakerId in newSpeakerIds.Where(s => !currentSpeakerIds.Contains(s)))
                            {
                                existingPresentation.AddSpeaker(speakerId);
                            }
                        }



                        if (existingPresentation.State == DomainModelState.Modified)
                        {
                            conference.UpdatePresentation(existingPresentation);
                            updatedPresentations.Add((existingPresentation, isScheduleChanged));
                            _logger.LogDebug("Updated presentation {PresentationId} - {PresentationTitle} (ExternalId: {ExternalId}), ScheduleChanged: {IsScheduleChanged}",
                                existingPresentation.Id,
                                existingPresentation.Title,
                                session.Id,
                                isScheduleChanged);
                        }
                        else
                        {
                            _logger.LogDebug("Presentation {PresentationId} - {PresentationTitle} (ExternalId: {ExternalId}) unchanged",
                                existingPresentation.Id, existingPresentation.Title, session.Id);
                        }
                    }
                    else
                    {
                        // Create new presentation
                        var presentation = Presentation.Create(
                            title,
                            description,
                            startDateTime,
                            endDateTime,
                            roomLocalId,
                            speakerIds,
                            session.Id);

                        try
                        {
                            conference.AddPresentation(presentation);
                            _logger.LogDebug("Added presentation {PresentationId} - {PresentationTitle} (ExternalId: {ExternalId})",
                                presentation.Id, presentation.Title, session.Id);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.LogWarning(ex, "Failed to add presentation {PresentationId}, skipping", presentation.Id);
                        }
                    }
                }
            }
        }

        // Save the updated conference
        await _conferenceRepository.UpdateAsync(conference, cancellationToken);

        // Publish integration events for updated presentations
        foreach (var (presentation, isScheduleChanged) in updatedPresentations)
        {
            var room = conference.Rooms.FirstOrDefault(r => r.Id == presentation.RoomId);
            var roomName = room?.Name ?? "Unknown";

            var integrationEvent = new PresentationUpdatedEvent
            {
                ConferenceId = conference.Id,
                PresentationId = presentation.Id,
                Title = presentation.Title,
                Abstract = presentation.Abstract,
                StartDateTime = presentation.StartDateTime,
                EndDateTime = presentation.EndDateTime,
                RoomId = presentation.RoomId,
                RoomName = roomName,
                SpeakerIds = presentation.SpeakerIds.ToList(),
                ExternalId = presentation.ExternalId,
                IsScheduleChanged = isScheduleChanged
            };

            await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

            _logger.LogInformation(
                "Published PresentationUpdatedEvent for presentation {PresentationId} - {PresentationTitle}, ScheduleChanged: {IsScheduleChanged}",
                presentation.Id,
                presentation.Title,
                isScheduleChanged);
        }

        _logger.LogInformation(
            "Successfully synchronized conference {ConferenceId} with Sessionize. Speakers: {SpeakerCount}, Rooms: {RoomCount}, Presentations: {PresentationCount}",
            conferenceId,
            conference.Speakers.Count,
            conference.Rooms.Count,
            conference.Presentations.Count);

        return new SessionizeSyncResult(
            conference.Id,
            conference.Speakers.Count,
            conference.Rooms.Count,
            conference.Presentations.Count);
    }
}
