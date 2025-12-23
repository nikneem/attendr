using HexMaster.Attendr.Conferences.Data.MongoDb.Models;
using HexMaster.Attendr.Conferences.DomainModels;

namespace HexMaster.Attendr.Conferences.Data.MongoDb.Mappers;

/// <summary>
/// Maps between Conference domain model and ConferenceDocument.
/// </summary>
internal static class ConferenceMapper
{
    public static ConferenceDocument ToDocument(Conference conference)
    {
        return new ConferenceDocument
        {
            Id = conference.Id,
            Title = conference.Title,
            City = conference.City,
            Country = conference.Country,
            StartDate = conference.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = conference.EndDate.ToDateTime(TimeOnly.MinValue),
            ImageUrl = conference.ImageUrl,
            SynchronizationSource = conference.SynchronizationSource != null
                ? new SynchronizationSourceDocument
                {
                    SourceType = (int)conference.SynchronizationSource.SourceType,
                    SourceUrl = conference.SynchronizationSource.SourceUrl,
                    ApiKey = conference.SynchronizationSource.ApiKey
                }
                : null,
            Rooms = conference.Rooms.Select(r => new RoomDocument
            {
                Id = r.Id,
                Name = r.Name,
                ExternalId = r.ExternalId,
                Capacity = r.Capacity
            }).ToList(),
            Speakers = conference.Speakers.Select(s => new SpeakerDocument
            {
                Id = s.Id,
                Name = s.Name,
                ExternalId = s.ExternalId,
                Company = s.Company,
                ProfilePictureUrl = s.ProfilePictureUrl
            }).ToList(),
            Presentations = conference.Presentations.Select(p => new PresentationDocument
            {
                Id = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                StartDateTime = p.StartDateTime,
                EndDateTime = p.EndDateTime,
                RoomId = p.RoomId,
                ExternalId = p.ExternalId,
                SpeakerIds = p.SpeakerIds.ToList()
            }).ToList()
        };
    }

    public static Conference ToDomain(ConferenceDocument document)
    {
        var synchronizationSource = document.SynchronizationSource != null
            ? SynchronizationSource.FromPersisted(
                (SynchronizationSourceType)document.SynchronizationSource.SourceType,
                document.SynchronizationSource.SourceUrl,
                document.SynchronizationSource.ApiKey)
            : null;

        var conference = Conference.FromPersisted(
            document.Id,
            document.Title,
            document.City,
            document.Country,
            DateOnly.FromDateTime(document.StartDate),
            DateOnly.FromDateTime(document.EndDate),
            document.ImageUrl,
            synchronizationSource);

        // Reconstitute rooms
        foreach (var roomDoc in document.Rooms)
        {
            var room = new Room(roomDoc.Id, roomDoc.Name, roomDoc.Capacity, roomDoc.ExternalId);
            conference.AddRoom(room);
        }

        // Reconstitute speakers
        foreach (var speakerDoc in document.Speakers)
        {
            var speaker = new Speaker(speakerDoc.Id, speakerDoc.Name, speakerDoc.Company, speakerDoc.ProfilePictureUrl, speakerDoc.ExternalId);
            conference.AddSpeaker(speaker);
        }

        // Reconstitute presentations
        foreach (var presDoc in document.Presentations)
        {
            var presentation = new Presentation(
                presDoc.Id,
                presDoc.Title,
                presDoc.Abstract,
                presDoc.StartDateTime,
                presDoc.EndDateTime,
                presDoc.RoomId,
                presDoc.SpeakerIds,
                presDoc.ExternalId);
            conference.AddPresentation(presentation);
        }

        return conference;
    }
}
