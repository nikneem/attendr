using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Data.Postgres.Migrations;

/// <summary>
/// Hosted service that runs database migrations on application startup.
/// </summary>
public sealed class MigrationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationHostedService> _logger;

    public MigrationHostedService(
        IServiceProvider serviceProvider,
        ILogger<MigrationHostedService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running database migrations on startup...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<DatabaseMigrationRunner>();
            await migrationRunner.RunMigrationsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to run database migrations. Application startup will continue, but the database may not be in the correct state.");
            // Note: We don't throw here to allow the application to start even if migrations fail
            // This can be changed to throw if you want to prevent startup on migration failure
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Nothing to do on shutdown
        return Task.CompletedTask;
    }
}
