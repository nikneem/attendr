using Dapr;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.IntegrationEvents.Events;
using Sessionize.Api.Client.Abstractions;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class EventHandlersEndpoints
{
    public static IEndpointRouteBuilder MapEventHandlersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/EventHandlers")
            .WithName("EventHandlers");

        group.MapPost("/ConferenceCreatedHandler", ConferenceCreatedHandler)
            .WithName("ConferenceCreatedHandler")
            .WithTopic("pubsub", "conference.created")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> ConferenceCreatedHandler(
        ConferenceCreatedEvent @event,
        IConferenceRepository conferenceRepository,
        ISessionizeApiClient sessionizeApiClient,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing ConferenceCreated event for conference {ConferenceId}", @event.ConferenceId);

            // Retrieve the conference from the repository
            var conference = await conferenceRepository.GetByIdAsync(@event.ConferenceId, cancellationToken);
            if (conference == null)
            {
                logger.LogWarning("Conference {ConferenceId} not found", @event.ConferenceId);
                return Results.BadRequest(new { error = "Conference not found" });
            }

            // Check if the conference has a Sessionize synchronization source
            if (conference.SynchronizationSource == null ||
                conference.SynchronizationSource.SourceType != SynchronizationSourceType.Sessionize ||
                string.IsNullOrWhiteSpace(conference.SynchronizationSource.ApiKey))
            {
                logger.LogInformation("Conference {ConferenceId} does not have Sessionize synchronization configured, skipping", @event.ConferenceId);
                return Results.Ok(new { message = "No Sessionize synchronization configured" });
            }

            logger.LogInformation("Fetching data from Sessionize for conference {ConferenceId} with API ID {ApiId}",
                @event.ConferenceId, conference.SynchronizationSource.ApiKey);

            // Fetch speakers
            var sessionizeSpeakers = await sessionizeApiClient.GetSpeakersListAsync(conference.SynchronizationSource.ApiKey, cancellationToken);
            var speakerIdMapping = new Dictionary<string, Guid>(); // Maps external ID to local GUID

            foreach (var sessionizeSpeaker in sessionizeSpeakers)
            {
                var externalId = sessionizeSpeaker.Id.ToString();

                // Check if speaker already exists by ExternalId
                var existingSpeaker = conference.Speakers.FirstOrDefault(s => s.ExternalId == externalId);

                if (existingSpeaker != null)
                {
                    // Speaker exists - update would go here if Speaker had update methods
                    speakerIdMapping[externalId] = existingSpeaker.Id;
                    logger.LogDebug("Speaker with ExternalId {ExternalId} already exists as {SpeakerId}", externalId, existingSpeaker.Id);
                }
                else
                {
                    // Create new speaker with local GUID
                    var localSpeakerId = Guid.NewGuid();
                    var speaker = new Speaker(
                        localSpeakerId,
                        sessionizeSpeaker.FullName ?? "Unknown",
                        sessionizeSpeaker.TagLine,
                        sessionizeSpeaker.ProfilePicture,
                        externalId);

                    try
                    {
                        conference.AddSpeaker(speaker);
                        speakerIdMapping[externalId] = localSpeakerId;
                        logger.LogDebug("Added speaker {SpeakerId} (ExternalId: {ExternalId}) - {SpeakerName}", speaker.Id, externalId, speaker.Name);
                    }
                    catch (InvalidOperationException ex)
                    {
                        logger.LogWarning(ex, "Failed to add speaker {SpeakerId}, skipping", localSpeakerId);
                    }
                }
            }

            // Fetch schedule to get rooms and sessions
            var scheduleGrid = await sessionizeApiClient.GetScheduleGridAsync(conference.SynchronizationSource.ApiKey, cancellationToken);
            var roomIdMapping = new Dictionary<string, Guid>(); // Maps room name (as external ID) to local GUID

            // First pass: collect all unique rooms
            foreach (var day in scheduleGrid)
            {
                foreach (var room in day.Rooms)
                {
                    var roomId = room.Id.ToString();

                    // Check if room already exists by ExternalId (using name as external ID)
                    var existingRoom = conference.Rooms.FirstOrDefault(r => r.ExternalId == roomId);

                    if (existingRoom != null)
                    {
                        roomIdMapping[roomId] = existingRoom.Id;
                        logger.LogDebug("Room {RoomName} already exists as {RoomId}", roomId, existingRoom.Id);
                    }
                    else if (!roomIdMapping.ContainsKey(roomId))
                    {
                        var roomEntity = new Room(Guid.NewGuid(), room.Name, 100, roomId); // Default capacity, using name as externalId

                        try
                        {
                            conference.AddRoom(roomEntity);
                            roomIdMapping[roomId] = roomEntity.Id;
                            logger.LogDebug("Added room {RoomId} (ExternalId: {ExternalId}) - {RoomName}", roomEntity.Id, roomId, roomEntity.Name);
                        }
                        catch (InvalidOperationException ex)
                        {
                            logger.LogWarning(ex, "Failed to add room {RoomName}, skipping", room.Name);
                        }
                    }
                }
            }

            // Second pass: add presentations
            foreach (var day in scheduleGrid)
            {
                foreach (var room in day.Rooms)
                {
                    foreach (var session in room.Sessions)
                    {
                        if (string.IsNullOrWhiteSpace(session.Id)) continue;

                        // Find the room ID
                        if (!roomIdMapping.TryGetValue(room.Name, out var roomId))
                        {
                            logger.LogWarning("Room {RoomName} not found for session {SessionId}, skipping", room.Name, session.Id);
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
                            logger.LogWarning("Session {SessionId} has no valid speakers, skipping", session.Id);
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
                            // Update existing presentation
                            existingPresentation.UpdateDetails(title, description, startDateTime, endDateTime);
                            existingPresentation.ChangeRoom(roomId);

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

                            if (existingPresentation.HasChanges)
                            {
                                logger.LogDebug("Updated presentation {PresentationId} - {PresentationTitle} (ExternalId: {ExternalId})", existingPresentation.Id, existingPresentation.Title, session.Id);
                            }
                            else
                            {
                                logger.LogDebug("Presentation {PresentationId} - {PresentationTitle} (ExternalId: {ExternalId}) unchanged",
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
                                roomId,
                                speakerIds,
                                session.Id);

                            try
                            {
                                conference.AddPresentation(presentation);
                                logger.LogDebug("Added presentation {PresentationId} - {PresentationTitle} (ExternalId: {ExternalId})",
                                    presentation.Id, presentation.Title, session.Id);
                            }
                            catch (InvalidOperationException ex)
                            {
                                logger.LogWarning(ex, "Failed to add presentation {PresentationId}, skipping", presentation.Id);
                            }
                        }
                    }
                }
            }

            // Save the updated conference
            await conferenceRepository.UpdateAsync(conference, cancellationToken);

            logger.LogInformation(
                "Successfully synchronized conference {ConferenceId} with Sessionize. Added {SpeakerCount} speakers, {RoomCount} rooms, {PresentationCount} presentations",
                @event.ConferenceId,
                conference.Speakers.Count,
                conference.Rooms.Count,
                conference.Presentations.Count);

            return Results.Ok(new
            {
                message = "Conference synchronized successfully",
                conferenceId = @event.ConferenceId,
                speakersCount = conference.Speakers.Count,
                roomsCount = conference.Rooms.Count,
                presentationsCount = conference.Presentations.Count
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing ConferenceCreated event for conference {ConferenceId}", @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to process conference created event", details = ex.Message });
        }
    }
}
