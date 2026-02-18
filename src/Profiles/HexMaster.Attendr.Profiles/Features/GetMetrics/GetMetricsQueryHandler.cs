using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Repositories;

namespace HexMaster.Attendr.Profiles.Features.GetMetrics;

public sealed class GetMetricsQueryHandler(
    IProfileRepository repository,
    IAttendrCacheClient cacheClient) : IQueryHandler<GetMetricsQuery, ProfileMetricsDto>
{
    public async Task<ProfileMetricsDto> Handle(GetMetricsQuery query, CancellationToken cancellationToken = default)
    {
        var result = await cacheClient.GetOrSetAsync(
            CacheKeys.Profiles.Metrics,
            async ct => (ProfileMetricsDto?)await repository.GetMetricsAsync(ct).ConfigureAwait(false),
            TimeSpan.FromHours(1),
            cancellationToken).ConfigureAwait(false);

        return result!;
    }
}
