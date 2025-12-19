namespace HexMaster.Attendr.Conferences.ListConferences;

public sealed record ListConferencesResult(
    List<ConferenceListItemDto> Conferences,
    int TotalCount,
    int PageNumber,
    int PageSize
);
