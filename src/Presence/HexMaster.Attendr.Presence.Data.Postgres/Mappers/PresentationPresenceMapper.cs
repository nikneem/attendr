using HexMaster.Attendr.Presence.Data.Postgres.Entities;
using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence.Data.Postgres.Mappers;

/// <summary>
/// Mapper for converting between PresentationPresence domain models and PostgreSQL entities.
/// </summary>
internal static class PresentationPresenceMapper
{
    /// <summary>
    /// Builds a composite ID from profile ID, conference ID, and presentation ID.
    /// </summary>
    public static string BuildId(Guid profileId, Guid conferenceId, Guid presentationId)
    {
        return $"{profileId}_{conferenceId}_{presentationId}";
    }

    /// <summary>
    /// Maps a PresentationPresence domain model to a PresentationPresenceEntity.
    /// </summary>
    public static PresentationPresenceEntity ToEntity(PresentationPresence presence)
    {
        ArgumentNullException.ThrowIfNull(presence);

        var speakers = presence.Speakers.Select(s => new SpeakerEmbedded(
            s.SpeakerId,
            s.Name,
            s.ProfilePictureUrl
        )).ToList();

        return new PresentationPresenceEntity(
            presence.ProfileId,
            presence.ConferenceId,
            presence.PresentationId,
            presence.Title,
            presence.Abstract,
            presence.Room,
            presence.StartDateTime,
            presence.EndDateTime,
            speakers,
            presence.IsRated,
            presence.IsFavorite,
            presence.IsCheckedIn,
            presence.CheckedInAt,
            presence.Rating
        );
    }

    /// <summary>
    /// Maps a PresentationPresenceEntity to a PresentationPresence domain model.
    /// </summary>
    public static PresentationPresence ToDomain(PresentationPresenceEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var speakers = entity.Speakers.Select(s => new PresentationSpeaker(
            s.SpeakerId,
            s.Name,
            s.ProfilePictureUrl
        )).ToList();

        return new PresentationPresence(
            entity.ProfileId,
            entity.ConferenceId,
            entity.PresentationId,
            entity.Title,
            entity.Abstract,
            entity.Room,
            entity.StartDateTime,
            entity.EndDateTime,
            speakers,
            entity.IsRated,
            entity.IsFavorite,
            entity.IsCheckedIn,
            entity.CheckedInAt,
            entity.Rating
        );
    }
}
