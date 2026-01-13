using System.Net;
using System.Net.Http.Json;
using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HexMaster.Attendr.Profiles.Integrations.Services;

public sealed class ProfilesIntegrationService : IProfilesIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly IAttendrCacheClient _cache;
    private readonly ILogger<ProfilesIntegrationService> _logger;
    private readonly TimeSpan _defaultTtl;

    public ProfilesIntegrationService(
        HttpClient httpClient,
        IAttendrCacheClient cache,
        ILogger<ProfilesIntegrationService> logger,
        IOptions<ProfilesIntegrationOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _defaultTtl = TimeSpan.FromSeconds(Math.Max(1, opts.CacheTtlSeconds));
    }

    public async Task<ResolveProfileResult?> ResolveProfile(string subjectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new ArgumentException("SubjectId cannot be null or whitespace.", nameof(subjectId));
        }

        var cacheKey = CacheKeys.Profiles.Subject(subjectId);
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

    public async Task<ProfileDetailsDto?> GetProfileDetails(string profileId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("ProfileId cannot be null or whitespace.", nameof(profileId));
        }

        var cacheKey = CacheKeys.Profiles.Details(profileId);

        try
        {
            // Use cache-aside pattern: GetOrSetAsync will check cache first,
            // then call the factory function if not found
            var profile = await _cache.GetOrSetAsync(
                cacheKey,
                async ct => await FetchProfileFromApi(profileId, ct),
                _defaultTtl,
                cancellationToken);

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get profile details for {ProfileId}", profileId);
            throw;
        }
    }

    private async Task<ProfileDetailsDto?> FetchProfileFromApi(string profileId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Fetching profile {ProfileId} from API", profileId);

            var response = await _httpClient.GetAsync($"/api/profiles-integration/{profileId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Profile {ProfileId} not found", profileId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var profile = await response.Content.ReadFromJsonAsync<ProfileDetailsDto>(cancellationToken);

            _logger.LogInformation("Successfully fetched profile {ProfileId} from API", profileId);

            return profile;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching profile {ProfileId} from API", profileId);
            throw;
        }
    }
}
