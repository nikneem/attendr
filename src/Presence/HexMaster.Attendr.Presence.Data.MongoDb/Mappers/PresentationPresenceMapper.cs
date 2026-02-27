using HexMaster.Attendr.Presence.Data.MongoDb.Models;
using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence.Data.MongoDb.Mappers;

public static class PresentationPresenceMapper
{
    public static PresentationPresenceDocument ToDocument(Guid profileId, Guid conferenceId, PresentationPresence domain)
    {
        return new PresentationPresenceDocument
        {
            Id = BuildId(profileId, conferenceId, domain.PresentationId),
            ProfileId = profileId,
            ConferenceId = conferenceId,
            PresentationId = domain.PresentationId,
            Title = domain.Title,
            Abstract = domain.Abstract,
            Room = domain.Room,
            StartDateTime = domain.StartDateTime.UtcDateTime,
            EndDateTime = domain.EndDateTime.UtcDateTime,
            IsRated = domain.IsRated,
            IsFavorite = domain.IsFavorite,
            IsCheckedIn = domain.IsCheckedIn,
            CheckedInAt = domain.CheckedInAt,
            Rating = domain.Rating,
            IsRecommended = domain.IsRecommended,
            IsPreferred = domain.IsPreferred,
            Speakers = domain.Speakers.Select(s => new PresentationSpeakerDocument
            {
                SpeakerId = s.SpeakerId,
                Name = s.Name,
                ProfilePictureUrl = s.ProfilePictureUrl
            }).ToList(),
            Topics = domain.Topics.Select(t => new PresentationTopicDocument
            {
                Key = t.Key,
                Name = t.Name
            }).ToList()
        };
    }

    public static PresentationPresence ToDomain(PresentationPresenceDocument doc)
    {
        var speakers = doc.Speakers.Select(s => new PresentationSpeaker(s.SpeakerId, s.Name, s.ProfilePictureUrl));
        var topics = doc.Topics.Select(t => new PresentationTopic(t.Key, t.Name));

        return new PresentationPresence(
            doc.ProfileId,
            doc.ConferenceId,
            doc.PresentationId,
            doc.Title,
            doc.Abstract,
            doc.Room,
            new DateTimeOffset(DateTime.SpecifyKind(doc.StartDateTime, DateTimeKind.Utc)),
            new DateTimeOffset(DateTime.SpecifyKind(doc.EndDateTime, DateTimeKind.Utc)),
            speakers,
            topics,
            doc.IsRated,
            doc.IsFavorite,
            doc.IsCheckedIn,
            doc.CheckedInAt,
            doc.Rating,
            doc.IsRecommended,
            doc.IsPreferred);
    }

    public static string BuildId(Guid profileId, Guid conferenceId, Guid presentationId) =>
        $"{profileId}:{conferenceId}:{presentationId}";
}
