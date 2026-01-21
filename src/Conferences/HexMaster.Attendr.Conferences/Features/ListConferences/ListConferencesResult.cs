namespace HexMaster.Attendr.Conferences.Features.ListConferences;

public sealed record ListConferencesResult(
    List<ConferenceListItemDto> Conferences,
    int TotalCount,
    int PageNumber,
    int PageSize
);

