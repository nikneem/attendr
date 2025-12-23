using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Data.MongoDb.Models;

namespace HexMaster.Attendr.Conferences.Data.MongoDb.Mappers;

/// <summary>
/// Maps ConferenceDocument directly to DTOs for read operations.
/// Bypasses domain model instantiation for performance.
/// </summary>
internal static class ConferenceDocumentToDtoMapper
{
    /// <summary>
    /// Maps a ConferenceDocument to ConferenceDetailsDto.
    /// </summary>
    public static ConferenceDetailsDto ToDetailsDto(ConferenceDocument document)
    {
        // Map synchronization source
        SynchronizationSourceDto? syncSourceDto = null;
        if (document.SynchronizationSource != null)
        {
            syncSourceDto = new SynchronizationSourceDto(
                ((DomainModels.SynchronizationSourceType)document.SynchronizationSource.SourceType).ToString(),
                document.SynchronizationSource.SourceLocationOrApiKey ?? string.Empty);
        }

        // Map speakers
        var speakerDtos = document.Speakers.Select(s => new SpeakerDto(
            s.Id,
            s.Name,
            s.ProfilePictureUrl)).ToList();

        // Create lookup dictionaries for efficient access
        var speakerLookup = document.Speakers.ToDictionary(s => s.Id);
        var roomLookup = document.Rooms.ToDictionary(r => r.Id);

        // Map presentations
        var presentationDtos = document.Presentations.Select(p =>
        {
            var roomName = roomLookup.TryGetValue(p.RoomId, out var room) ? room.Name : "Unknown";
            var presentationSpeakers = p.SpeakerIds
                .Where(speakerId => speakerLookup.ContainsKey(speakerId))
                .Select(speakerId =>
                {
                    var speaker = speakerLookup[speakerId];
                    return new SpeakerDto(speaker.Id, speaker.Name, speaker.ProfilePictureUrl);
                })
                .ToList();

            return new PresentationDto(
                p.Id,
                p.Title,
                p.Abstract,
                p.StartDateTime,
                p.EndDateTime,
                roomName,
                presentationSpeakers);
        }).ToList();

        return new ConferenceDetailsDto(
            document.Id,
            document.Title,
            document.City,
            document.Country,
            DateOnly.FromDateTime(document.StartDate),
            DateOnly.FromDateTime(document.EndDate),
            document.ImageUrl,
            syncSourceDto,
            speakerDtos,
            presentationDtos);
    }
}
