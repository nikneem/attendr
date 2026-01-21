namespace HexMaster.Attendr.Conferences.Features.ListConferences;

public sealed record ListConferencesQuery(
    string? SearchQuery = null,
    int? PageSize = null,
    int PageNumber = 1,
    bool ShowHidden = false
);
