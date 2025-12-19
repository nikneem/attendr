namespace HexMaster.Attendr.Conferences.ListConferences;

public sealed record ListConferencesQuery(
    string? SearchQuery = null,
    int? PageSize = null,
    int PageNumber = 1
);
