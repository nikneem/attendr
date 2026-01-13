using HexMaster.Attendr.Groups.Data.Postgress.Entities;
using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Data.Postgress.Mappers;

/// <summary>
/// Mapper for converting between CheckIn domain models and PostgreSQL entities.
/// </summary>
internal static class CheckInMapper
{
    public static CheckInEntity ToEntity(CheckIn checkIn)
    {
        ArgumentNullException.ThrowIfNull(checkIn);

        var presentationData = new PresentationDataEntity(
            checkIn.PresentationData.Id,
            checkIn.PresentationData.Title,
            checkIn.PresentationData.Abstract,
            checkIn.PresentationData.Room,
            checkIn.PresentationData.StartDateTime,
            checkIn.PresentationData.EndDateTime,
            checkIn.PresentationData.Speakers.Select(s => new PresentationSpeakerEntity(
                s.Id,
                s.Name,
                s.ProfilePictureUrl
            )).ToList()
        );

        var members = checkIn.Members.Select(m => new CheckedInMemberEntity(
            m.Id,
            m.Name,
            m.ProfilePictureUrl
        )).ToList();

        return new CheckInEntity(
            checkIn.Id,
            checkIn.GroupId,
            checkIn.ConferenceId,
            checkIn.PresentationId,
            presentationData,
            members,
            checkIn.Expiration
        );
    }

    public static CheckIn ToDomain(CheckInEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var speakers = entity.PresentationData.Speakers.Select(s => new PresentationSpeaker(
            s.Id,
            s.Name,
            s.ProfilePictureUrl
        )).ToList();

        var presentationData = new PresentationData(
            entity.PresentationData.Id,
            entity.PresentationData.Title,
            entity.PresentationData.Abstract,
            entity.PresentationData.Room,
            entity.PresentationData.StartDateTime,
            entity.PresentationData.EndDateTime,
            speakers
        );

        var members = entity.MemberData.Select(m => new CheckedInMember(
            m.Id,
            m.Name,
            m.ProfilePictureUrl
        )).ToList();

        return CheckIn.FromPersisted(
            entity.Id,
            entity.GroupId,
            entity.ConferenceId,
            entity.PresentationId,
            presentationData,
            entity.Expiration,
            members
        );
    }
}
