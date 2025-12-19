using System.Net;
using System.Net.Http.Json;
using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using Microsoft.Extensions.Options;

namespace HexMaster.Attendr.Profiles.Integrations.Services;

public sealed class ProfilesIntegrationService : IProfilesIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly IAttendrCacheClient _cache;
    private readonly TimeSpan _defaultTtl;

    public ProfilesIntegrationService(HttpClient httpClient, IAttendrCacheClient cache, IOptions<ProfilesIntegrationOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _defaultTtl = TimeSpan.FromSeconds(Math.Max(1, opts.CacheTtlSeconds));
    }

    public async Task<ResolveProfileResult?> ResolveProfile(string subjectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new ArgumentException("SubjectId cannot be null or whitespace.", nameof(subjectId));
        }

        var cacheKey = $"profiles:subject:{subjectId}";
        return await _cache.GetOrSetAsync<ResolveProfileResult>(
            cacheKey,
            async ct =>
            {
                var request = new ResolveProfileRequest(subjectId);
                var response = await _httpClient.PostAsJsonAsync("/api/profiles-integration/resolve", request, ct).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<ResolveProfileResult>(cancellationToken: ct).ConfigureAwait(false);
                return result;
            },
            ttl: _defaultTtl,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
