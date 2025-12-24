using HexMaster.Attendr.Presence.Data.MongoDb.Models;
using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence.Data.MongoDb.Mappers;

public static class ConferencePresenceMapper
{
    public static ConferencePresenceDocument ToDocument(ConferencePresence domain)
    {
        return new ConferencePresenceDocument
        {
            Id = BuildId(domain.ProfileId, domain.ConferenceId),
            ProfileId = domain.ProfileId,
            ConferenceId = domain.ConferenceId,
            ConferenceName = domain.ConferenceName,
            Location = domain.Location,
            StartDate = domain.StartDate,
            EndDate = domain.EndDate,
            IsFollowing = domain.IsFollowing,
            IsAttending = domain.IsAttending
        };
    }

    public static ConferencePresence ToDomain(ConferencePresenceDocument doc)
    {
        return new ConferencePresence(
            doc.ConferenceId,
            doc.ConferenceName,
            doc.Location,
            doc.StartDate,
            doc.EndDate,
            doc.ProfileId,
            isFollowing: doc.IsFollowing,
            isAttending: doc.IsAttending,
            presentations: null);
    }

    public static string BuildId(Guid profileId, Guid conferenceId) => $"{profileId}:{conferenceId}";
}
