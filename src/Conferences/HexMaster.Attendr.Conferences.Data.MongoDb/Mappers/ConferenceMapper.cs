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
                Capacity = r.Capacity
            }).ToList(),
            Speakers = conference.Speakers.Select(s => new SpeakerDocument
            {
                Id = s.Id,
                Name = s.Name,
                Biography = s.Biography,
                Company = s.Company
            }).ToList(),
            Presentations = conference.Presentations.Select(p => new PresentationDocument
            {
                Id = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                StartDateTime = p.StartDateTime,
                EndDateTime = p.EndDateTime,
                RoomId = p.RoomId,
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
            synchronizationSource);

        // Reconstitute rooms
        foreach (var roomDoc in document.Rooms)
        {
            var room = new Room(roomDoc.Id, roomDoc.Name, roomDoc.Capacity);
            conference.AddRoom(room);
        }

        // Reconstitute speakers
        foreach (var speakerDoc in document.Speakers)
        {
            var speaker = new Speaker(speakerDoc.Id, speakerDoc.Name, speakerDoc.Biography, speakerDoc.Company);
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
                presDoc.SpeakerIds);
            conference.AddPresentation(presentation);
        }

        return conference;
    }
}
