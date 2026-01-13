using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Integrations.Abstractions;
using HexMaster.Attendr.Core.Cache;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace HexMaster.Attendr.Conferences.Integrations.Services;

/// <summary>
/// Implementation of conferences integration service with cache-aside pattern.
/// </summary>
public sealed class ConferencesIntegrationService : IConferencesIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly IAttendrCacheClient _cacheClient;
    private readonly ILogger<ConferencesIntegrationService> _logger;
    private const string ConferenceDetailsCacheKeyPrefix = "conference:details:";
    private const string PresentationDetailsCacheKeyPrefix = "presentation:details:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public ConferencesIntegrationService(
        HttpClient httpClient,
        IAttendrCacheClient cacheClient,
        ILogger<ConferencesIntegrationService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cacheClient = cacheClient ?? throw new ArgumentNullException(nameof(cacheClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ConferenceDetailsDto?> GetConferenceDetails(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ConferenceDetailsCacheKeyPrefix}{id}";

        try
        {
            // Use cache-aside pattern: GetOrSetAsync will check cache first,
            // then call the factory function if not found
            var conference = await _cacheClient.GetOrSetAsync(
                cacheKey,
                async ct => await FetchConferenceFromApi(id, ct),
                CacheTtl,
                cancellationToken);

            return conference;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get conference details for {ConferenceId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PresentationDto?> GetPresentationDetails(Guid conferenceId, Guid presentationId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{PresentationDetailsCacheKeyPrefix}{conferenceId}:{presentationId}";

        try
        {
            // Use cache-aside pattern: GetOrSetAsync will check cache first,
            // then call the factory function if not found
            var presentation = await _cacheClient.GetOrSetAsync(
                cacheKey,
                async ct => await FetchPresentationFromApi(conferenceId, presentationId, ct),
                CacheTtl,
                cancellationToken);

            return presentation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get presentation details for conference {ConferenceId} and presentation {PresentationId}", 
                conferenceId, presentationId);
            throw;
        }
    }

    private async Task<ConferenceDetailsDto?> FetchConferenceFromApi(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Fetching conference {ConferenceId} from API", id);

            var response = await _httpClient.GetAsync($"/api/conferences-integration/{id}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Conference {ConferenceId} not found", id);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var conference = await response.Content.ReadFromJsonAsync<ConferenceDetailsDto>(cancellationToken);

            _logger.LogInformation("Successfully fetched conference {ConferenceId} from API", id);

            return conference;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching conference {ConferenceId} from API", id);
            throw;
        }
    }

    private async Task<PresentationDto?> FetchPresentationFromApi(Guid conferenceId, Guid presentationId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Fetching presentation {PresentationId} for conference {ConferenceId} from API", 
                presentationId, conferenceId);

            var response = await _httpClient.GetAsync(
                $"/api/conferences-integration/{conferenceId}/presentations/{presentationId}", 
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Presentation {PresentationId} for conference {ConferenceId} not found", 
                    presentationId, conferenceId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var presentation = await response.Content.ReadFromJsonAsync<PresentationDto>(cancellationToken);

            _logger.LogInformation("Successfully fetched presentation {PresentationId} for conference {ConferenceId} from API", 
                presentationId, conferenceId);

            return presentation;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching presentation {PresentationId} for conference {ConferenceId} from API", 
                presentationId, conferenceId);
            throw;
        }
    }
}
