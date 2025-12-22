using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.ListConferences;

public sealed class ListConferencesQueryHandler(IConferenceRepository repository)
    : IQueryHandler<ListConferencesQuery, ListConferencesResult>
{
    public async Task<ListConferencesResult> Handle(
        ListConferencesQuery query,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Conferences.StartActivity("ListConferencesQueryHandler.HandleAsync");
        activity?.SetTag("search_query", query.SearchQuery ?? "null");
        activity?.SetTag("page_number", query.PageNumber);
        activity?.SetTag("page_size", query.PageSize ?? PaginationConstants.DefaultPageSize);

        var pageSize = PaginationConstants.NormalizePageSize(query.PageSize);

        var (conferences, totalCount) = await repository.ListConferencesAsync(
            query.SearchQuery,
            query.PageNumber,
            pageSize,
            cancellationToken);

        var items = conferences.Select(c => new ConferenceListItemDto(
            c.Id,
            c.Title,
            c.City,
            c.Country,
            c.StartDate,
            c.EndDate,
            c.SynchronizationSource != null
        )).ToList();

        activity?.SetTag("total_count", totalCount);
        activity?.SetTag("returned_count", items.Count);

        return new ListConferencesResult(items, totalCount, query.PageNumber, pageSize);
    }

}
