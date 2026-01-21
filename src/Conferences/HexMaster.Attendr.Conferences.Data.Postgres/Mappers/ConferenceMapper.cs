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
        Dictionary<Guid, List<Guid>> presentationSpeakers)
    {
        ArgumentNullException.ThrowIfNull(conferenceEntity);
        ArgumentNullException.ThrowIfNull(rooms);
        ArgumentNullException.ThrowIfNull(speakers);
        ArgumentNullException.ThrowIfNull(presentations);
        ArgumentNullException.ThrowIfNull(presentationSpeakers);

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
            syncSource);

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

            var presentation = Presentation.FromPersisted(
                presentationEntity.Id,
                presentationEntity.Title,
                presentationEntity.Abstract,
                presentationEntity.StartDateTime,
                presentationEntity.EndDateTime,
                presentationEntity.RoomId,
                speakerIds,
                presentationEntity.ExternalId,
                presentationEntity.IsAnalysed);

            conference.AddPresentation(presentation);
        }

        return conference;
    }
}
