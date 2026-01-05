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
            StartDateTime = domain.StartDateTime,
            EndDateTime = domain.EndDateTime,
            IsRated = domain.IsRated,
            IsFavorite = domain.IsFavorite,
            IsCheckedIn = domain.IsCheckedIn,
            Rating = domain.Rating,
            Speakers = domain.Speakers.Select(s => new PresentationSpeakerDocument
            {
                SpeakerId = s.SpeakerId,
                Name = s.Name,
                ProfilePictureUrl = s.ProfilePictureUrl
            }).ToList()
        };
    }

    public static PresentationPresence ToDomain(PresentationPresenceDocument doc)
    {
        var speakers = doc.Speakers.Select(s => new PresentationSpeaker(s.SpeakerId, s.Name, s.ProfilePictureUrl));

        return new PresentationPresence(
            doc.ProfileId,
            doc.ConferenceId,
            doc.PresentationId,
            doc.Title,
            doc.Abstract,
            doc.Room,
            doc.StartDateTime,
            doc.EndDateTime,
            speakers,
            doc.IsRated,
            doc.IsFavorite,
            doc.IsCheckedIn,
            doc.Rating);
    }

    public static string BuildId(Guid profileId, Guid conferenceId, Guid presentationId) =>
        $"{profileId}:{conferenceId}:{presentationId}";
}
