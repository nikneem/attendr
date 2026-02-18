using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.GetMetrics;

public sealed class GetMetricsQueryHandler(
    IConferenceRepository repository,
    IAttendrCacheClient cacheClient) : IQueryHandler<GetMetricsQuery, ConferenceMetricsDto>
{
    public async Task<ConferenceMetricsDto> Handle(GetMetricsQuery query, CancellationToken cancellationToken = default)
    {
        var result = await cacheClient.GetOrSetAsync(
            CacheKeys.Conferences.Metrics,
            async ct => (ConferenceMetricsDto?)await repository.GetMetricsAsync(ct).ConfigureAwait(false),
            TimeSpan.FromHours(1),
            cancellationToken).ConfigureAwait(false);

        return result!;
    }
}
