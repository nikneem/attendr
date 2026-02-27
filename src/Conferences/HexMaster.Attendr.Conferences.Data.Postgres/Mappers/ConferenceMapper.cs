using HexMaster.Attendr.Conferences.Data.Postgres.Entities;
using HexMaster.Attendr.Conferences.DomainModels;

namespace HexMaster.Attendr.Conferences.Data.Postgres.Mappers;

/// <summary>
/// Mapper for converting between Conference domain models and PostgreSQL entities.
/// </summary>
internal static class ConferenceMapper
{
    /// <summary>
    /// Maps a Conference domain model to a ConferenceEntity.
    /// </summary>
    public static ConferenceEntity ToEntity(Conference conference)
    {
        ArgumentNullException.ThrowIfNull(conference);

        return new ConferenceEntity
        {
            Id = conference.Id,
            Title = conference.Title,
            City = conference.City,
            Country = conference.Country,
            StartDate = conference.StartDate,
            EndDate = conference.EndDate,
            ImageUrl = conference.ImageUrl,
            IsVisible = conference.IsVisible,
            CreatedByProfileId = conference.CreatedByProfileId,
            SyncSourceType = conference.SynchronizationSource?.SourceType != null
                ? (int)conference.SynchronizationSource.SourceType
                : null,
            SyncSourceLocationOrApiKey = conference.SynchronizationSource?.SourceLocationOrApiKey
        };
    }

    /// <summary>
    /// Maps entities to a Conference domain model.
    /// </summary>
    public static Conference ToDomain(
        ConferenceEntity conferenceEntity,
        List<RoomEntity> rooms,
        List<SpeakerEntity> speakers,
        List<PresentationEntity> presentations,
        Dictionary<Guid, List<Guid>> presentationSpeakers,
        Dictionary<Guid, List<(string Key, string Name)>> presentationTopics)
    {
        ArgumentNullException.ThrowIfNull(conferenceEntity);
        ArgumentNullException.ThrowIfNull(rooms);
        ArgumentNullException.ThrowIfNull(speakers);
        ArgumentNullException.ThrowIfNull(presentations);
        ArgumentNullException.ThrowIfNull(presentationSpeakers);
        ArgumentNullException.ThrowIfNull(presentationTopics);

        // Create the conference
        SynchronizationSource? syncSource = null;
        if (conferenceEntity.SyncSourceType.HasValue)
        {
            syncSource = SynchronizationSource.FromPersisted(
                (SynchronizationSourceType)conferenceEntity.SyncSourceType.Value,
                conferenceEntity.SyncSourceLocationOrApiKey);
        }

        var conference = Conference.FromPersisted(
            conferenceEntity.Id,
            conferenceEntity.Title,
            conferenceEntity.City,
            conferenceEntity.Country,
            conferenceEntity.StartDate,
            conferenceEntity.EndDate,
            conferenceEntity.ImageUrl,
            conferenceEntity.IsVisible,
            syncSource,
            conferenceEntity.CreatedByProfileId);

        // Add rooms
        foreach (var roomEntity in rooms)
        {
            var room = Room.FromPersisted(
                roomEntity.Id,
                roomEntity.Name,
                roomEntity.Capacity,
                roomEntity.ExternalId);
            conference.AddRoom(room);
        }

        // Add speakers
        foreach (var speakerEntity in speakers)
        {
            var speaker = Speaker.FromPersisted(
                speakerEntity.Id,
                speakerEntity.Name,
                speakerEntity.Company,
                speakerEntity.ProfilePictureUrl,
                speakerEntity.ExternalId);
            conference.AddSpeaker(speaker);
        }

        // Add presentations
        foreach (var presentationEntity in presentations)
        {
            var speakerIds = presentationSpeakers.ContainsKey(presentationEntity.Id)
                ? presentationSpeakers[presentationEntity.Id]
                : new List<Guid>();

            // Get speaker objects for this presentation
            var presentationSpeakersForPresentation = speakers
                .Where(s => speakerIds.Contains(s.Id))
                .Select(s => Speaker.FromPersisted(s.Id, s.Name, s.Company, s.ProfilePictureUrl, s.ExternalId))
                .ToList();

            var topics = presentationTopics.ContainsKey(presentationEntity.Id)
                ? presentationTopics[presentationEntity.Id].Select(t => new PresentationTopic(t.Key, t.Name)).ToList()
                : new List<PresentationTopic>();

            var presentationRoom = conference.Rooms.First(r => r.Id == presentationEntity.RoomId);

            var presentation = Presentation.FromPersisted(
                presentationEntity.Id,
                presentationEntity.Title,
                presentationEntity.Abstract,
                presentationEntity.StartDateTime,
                presentationEntity.EndDateTime,
                presentationRoom,
                presentationSpeakersForPresentation,
                presentationEntity.ExternalId,
                topics,
                presentationEntity.IsAnalysed);

            conference.AddPresentation(presentation);
        }

        return conference;
    }
}
