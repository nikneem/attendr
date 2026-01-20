using HexMaster.Attendr.Conferences.Abstractions.Services;

namespace HexMaster.Attendr.Conferences.Api.BackgroundServices;

public sealed class ConferenceSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ConferenceSyncBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Conference sync background service started. Interval: {SyncInterval}",
            SyncInterval);

        // Run immediately on startup to keep data fresh after deployments.
        await RunSyncIterationAsync(stoppingToken);

        using var timer = new PeriodicTimer(SyncInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunSyncIterationAsync(stoppingToken);
        }
    }

    private async Task RunSyncIterationAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IConferenceRepository>();
            var syncService = scope.ServiceProvider.GetRequiredService<ISessionizeSyncService>();

            var conferenceIds = await repository.ListActiveConferenceIdsWithSyncSourceAsync(cancellationToken);
            if (conferenceIds.Count == 0)
            {
                logger.LogDebug("No active conferences with synchronization source found");
                return;
            }

            foreach (var conferenceId in conferenceIds)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    logger.LogInformation("Synchronizing conference {ConferenceId} from background job", conferenceId);
                    await syncService.SynchronizeConferenceAsync(conferenceId, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to synchronize conference {ConferenceId}", conferenceId);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Graceful shutdown requested.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Background conference synchronization iteration failed");
        }
    }
}
