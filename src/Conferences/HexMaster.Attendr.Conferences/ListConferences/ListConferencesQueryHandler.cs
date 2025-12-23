using System.Diagnostics;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.ListConferences;

public sealed class ListConferencesQueryHandler : IQueryHandler<ListConferencesQuery, ListConferencesResult>
{
    private readonly IConferenceRepository _repository;
    private readonly ConferenceMetrics _metrics;
    private readonly ILogger<ListConferencesQueryHandler> _logger;

    public ListConferencesQueryHandler(
        IConferenceRepository repository,
        ConferenceMetrics metrics,
        ILogger<ListConferencesQueryHandler> logger)
    {
        _repository = repository;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<ListConferencesResult> Handle(
        ListConferencesQuery query,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Conferences.StartActivity("ListConferencesQueryHandler.HandleAsync");
        activity?.SetTag("search_query", query.SearchQuery ?? "null");
        activity?.SetTag("page_number", query.PageNumber);
        activity?.SetTag("page_size", query.PageSize ?? PaginationConstants.DefaultPageSize);

        var pageSize = PaginationConstants.NormalizePageSize(query.PageSize);

        var (conferences, totalCount) = await _repository.ListConferencesAsync(
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
            c.ImageUrl,
            c.SynchronizationSource != null
        )).ToList();

        activity?.SetTag("total_count", totalCount);
        activity?.SetTag("returned_count", items.Count);
        activity?.SetStatus(ActivityStatusCode.Ok);

        _metrics.RecordConferencesListed(items.Count);

        _logger.LogInformation("Listed {Count} conferences (page {Page}, total {Total})", items.Count, query.PageNumber, totalCount);

        return new ListConferencesResult(items, totalCount, query.PageNumber, pageSize);
    }
}
