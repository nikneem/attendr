namespace HexMaster.Attendr.Conferences.ListConferences;

public sealed record ConferenceListItemDto(
    Guid Id,
    string Title,
    string? City,
    string? Country,
    DateOnly StartDate,
    DateOnly EndDate,
    bool HasSynchronizationSource
);
