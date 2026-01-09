using HexMaster.Attendr.Presence.Data.Postgres.Entities;
using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence.Data.Postgres.Mappers;

/// <summary>
/// Mapper for converting between ConferencePresence domain models and PostgreSQL entities.
/// </summary>
internal static class ConferencePresenceMapper
{
    /// <summary>
    /// Builds a composite ID from profile ID and conference ID.
    /// </summary>
    public static string BuildId(Guid profileId, Guid conferenceId)
    {
        return $"{profileId}_{conferenceId}";
    }

    /// <summary>
    /// Maps a ConferencePresence domain model to a ConferencePresenceEntity.
    /// </summary>
    public static ConferencePresenceEntity ToEntity(ConferencePresence presence)
    {
        ArgumentNullException.ThrowIfNull(presence);

        var presentations = presence.Presentations.Select(p => new PresentationPresenceEmbedded(
            p.PresentationId,
            p.Title,
            p.Abstract,
            p.Room,
            p.StartDateTime,
            p.EndDateTime,
            p.Speakers.Select(s => new SpeakerEmbedded(s.SpeakerId, s.Name, s.ProfilePictureUrl)).ToList(),
            p.IsRated,
            p.IsFavorite,
            p.IsCheckedIn,
            p.CheckedInAt,
            p.Rating
        )).ToList();

        return new ConferencePresenceEntity(
            presence.ProfileId,
            presence.ConferenceId,
            presence.ConferenceName,
            presence.Location,
            presence.ImageUrl,
            presence.StartDate,
            presence.EndDate,
            presence.IsFollowing,
            presence.IsAttending,
            presentations
        );
    }

    /// <summary>
    /// Maps a ConferencePresenceEntity to a ConferencePresence domain model.
    /// </summary>
    public static ConferencePresence ToDomain(ConferencePresenceEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var presentations = entity.Presentations.Select(p =>
        {
            var speakers = p.Speakers.Select(s => new PresentationSpeaker(s.SpeakerId, s.Name, s.ProfilePictureUrl)).ToList();

            return new PresentationPresence(
                entity.ProfileId,
                entity.ConferenceId,
                p.PresentationId,
                p.Title,
                p.Abstract,
                p.Room,
                p.StartDateTime,
                p.EndDateTime,
                speakers,
                p.IsRated,
                p.IsFavorite,
                p.IsCheckedIn,
                p.CheckedInAt,
                p.Rating
            );
        }).ToList();

        return new ConferencePresence(
            entity.ConferenceId,
            entity.ConferenceName,
            entity.Location,
            entity.StartDate,
            entity.EndDate,
            entity.ProfileId,
            entity.ImageUrl,
            entity.IsFollowing,
            entity.IsAttending,
            presentations
        );
    }
}
