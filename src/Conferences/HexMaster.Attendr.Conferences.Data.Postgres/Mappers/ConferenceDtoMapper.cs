using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Data.Postgres.Entities;
using HexMaster.Attendr.Conferences.DomainModels;

namespace HexMaster.Attendr.Conferences.Data.Postgres.Mappers;

/// <summary>
/// Mapper for converting PostgreSQL entities directly to DTOs for read operations.
/// </summary>
internal static class ConferenceDtoMapper
{
    /// <summary>
    /// Maps entities to a ConferenceDetailsDto for efficient read operations.
    /// </summary>
    public static ConferenceDetailsDto ToDetailsDto(
        ConferenceEntity conferenceEntity,
        List<RoomEntity> rooms,
        List<SpeakerEntity> speakers,
        List<PresentationEntity> presentations,
        Dictionary<Guid, List<Guid>> presentationSpeakerMap)
    {
        ArgumentNullException.ThrowIfNull(conferenceEntity);
        ArgumentNullException.ThrowIfNull(rooms);
        ArgumentNullException.ThrowIfNull(speakers);
        ArgumentNullException.ThrowIfNull(presentations);
        ArgumentNullException.ThrowIfNull(presentationSpeakerMap);

        // Create a dictionary for quick room lookup
        var roomDictionary = rooms.ToDictionary(r => r.Id, r => r.Name);

        var speakerDtos = speakers.Select(s => new SpeakerDto(
            s.Id,
            s.Name,
            s.ProfilePictureUrl
        )).ToList();

        // Create a dictionary for quick speaker lookup
        var speakerDictionary = speakerDtos.ToDictionary(s => s.Id);

        var presentationDtos = presentations.Select(p =>
        {
            var speakerIds = presentationSpeakerMap.ContainsKey(p.Id)
                ? presentationSpeakerMap[p.Id]
                : new List<Guid>();

            var presentationSpeakers = speakerIds
                .Where(id => speakerDictionary.ContainsKey(id))
                .Select(id => speakerDictionary[id])
                .ToList();

            var roomName = roomDictionary.TryGetValue(p.RoomId, out var name) ? name : "Unknown";

            return new PresentationDto(
                p.Id,
                p.Title,
                p.Abstract,
                p.StartDateTime,
                p.EndDateTime,
                roomName,
                presentationSpeakers
            );
        }).ToList();

        SynchronizationSourceDto? syncSource = null;
        if (conferenceEntity.SyncSourceType.HasValue && 
            !string.IsNullOrWhiteSpace(conferenceEntity.SyncSourceLocationOrApiKey))
        {
            var sourceType = (SynchronizationSourceType)conferenceEntity.SyncSourceType.Value;
            syncSource = new SynchronizationSourceDto(
                sourceType.ToString(),
                conferenceEntity.SyncSourceLocationOrApiKey
            );
        }

        return new ConferenceDetailsDto(
            conferenceEntity.Id,
            conferenceEntity.Title,
            conferenceEntity.City,
            conferenceEntity.Country,
            conferenceEntity.StartDate,
            conferenceEntity.EndDate,
            conferenceEntity.ImageUrl,
            conferenceEntity.IsVisible,
            syncSource,
            speakerDtos,
            presentationDtos
        );
    }
}
