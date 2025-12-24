namespace HexMaster.Attendr.Conferences.ListConferences;

public sealed record ConferenceListItemDto(
    Guid Id,
    string Title,
    string? City,
    string? Country,
    DateOnly StartDate,
    DateOnly EndDate,
    string? ImageUrl,
    bool HasSynchronizationSource,
    int SpeakersCount,
    int RoomsCount,
    int PresentationsCount
);
