using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Services;

/// <summary>
/// Background service that periodically scans for unanalyzed presentations and triggers topic analysis.
/// </summary>
public sealed class PresentationAnalysisBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PresentationAnalysisBackgroundService> _logger;
    private readonly TimeSpan _scanInterval = TimeSpan.FromHours(1);
    private readonly int _batchSize = 50; // Process 50 presentations per scan

    public PresentationAnalysisBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PresentationAnalysisBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Presentation Analysis Background Service started");

        // Wait 30 seconds before first scan to allow services to initialize
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanAndAnalyzePresentationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during presentation analysis scan");
            }

            // Wait for the next scan interval
            await Task.Delay(_scanInterval, stoppingToken);
        }

        _logger.LogInformation("Presentation Analysis Background Service stopped");
    }

    private async Task ScanAndAnalyzePresentationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var topicsRepository = scope.ServiceProvider.GetRequiredService<ITopicsRepository>();
        var analysisService = scope.ServiceProvider.GetRequiredService<PresentationTopicsAnalysisService>();

        _logger.LogInformation("Starting scan for unanalyzed presentations");

        var unanalyzedPresentations = await topicsRepository.GetUnanalysedPresentationsAsync(_batchSize, cancellationToken);

        if (unanalyzedPresentations.Count == 0)
        {
            _logger.LogInformation("No unanalyzed presentations found");
            return;
        }

        _logger.LogInformation("Found {Count} unanalyzed presentations to process", unanalyzedPresentations.Count);

        var successCount = 0;
        var failureCount = 0;

        foreach (var (conferenceId, presentation) in unanalyzedPresentations)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                _logger.LogInformation("Analyzing presentation {PresentationId}: {Title}",
                    presentation.Id, presentation.Title);

                await analysisService.AnalyzeAsync(conferenceId, presentation, cancellationToken);
                successCount++;

                // Small delay between API calls to respect rate limits
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze presentation {PresentationId}: {Title}",
                    presentation.Id, presentation.Title);
                failureCount++;
            }
        }

        _logger.LogInformation("Presentation analysis scan completed. Success: {SuccessCount}, Failed: {FailureCount}",
            successCount, failureCount);
    }
}
