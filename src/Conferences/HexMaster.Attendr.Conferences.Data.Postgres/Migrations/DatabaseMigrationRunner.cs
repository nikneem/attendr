using System.Reflection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace HexMaster.Attendr.Conferences.Data.Postgres.Migrations;

/// <summary>
/// Service for running database migrations from embedded SQL scripts.
/// </summary>
public sealed class DatabaseMigrationRunner
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<DatabaseMigrationRunner> _logger;

    public DatabaseMigrationRunner(
        NpgsqlDataSource dataSource,
        ILogger<DatabaseMigrationRunner> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs all pending migrations.
    /// </summary>
    public async Task RunMigrationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting database migrations...");

        try
        {
            // Ensure migrations table exists
            await EnsureMigrationsTableExistsAsync(cancellationToken);

            // Get all migration scripts from embedded resources
            var migrations = GetMigrationScripts();

            if (migrations.Count == 0)
            {
                _logger.LogInformation("No migration scripts found");
                return;
            }

            // Execute each migration
            foreach (var (migrationName, script) in migrations.OrderBy(m => m.Key))
            {
                await ExecuteMigrationAsync(migrationName, script, cancellationToken);
            }

            _logger.LogInformation("Database migrations completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run database migrations");
            throw;
        }
    }

    private async Task EnsureMigrationsTableExistsAsync(CancellationToken cancellationToken)
    {
        var sql = @"
            CREATE TABLE IF NOT EXISTS _migrations (
                id SERIAL PRIMARY KEY,
                migration_name VARCHAR(255) NOT NULL UNIQUE,
                applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Migrations table ensured");
    }

    private async Task ExecuteMigrationAsync(string migrationName, string script, CancellationToken cancellationToken)
    {
        // Check if migration has already been applied
        if (await IsMigrationAppliedAsync(migrationName, cancellationToken))
        {
            _logger.LogDebug("Migration {MigrationName} already applied, skipping", migrationName);
            return;
        }

        _logger.LogInformation("Applying migration: {MigrationName}", migrationName);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Execute the migration script
            await using var command = new NpgsqlCommand(script, connection, transaction);
            await command.ExecuteNonQueryAsync(cancellationToken);

            // Record the migration
            var recordSql = "INSERT INTO _migrations (migration_name) VALUES (@migrationName)";
            await using var recordCommand = new NpgsqlCommand(recordSql, connection, transaction);
            recordCommand.Parameters.AddWithValue("@migrationName", migrationName);
            await recordCommand.ExecuteNonQueryAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Migration {MigrationName} applied successfully", migrationName);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to apply migration {MigrationName}", migrationName);
            throw;
        }
    }

    private async Task<bool> IsMigrationAppliedAsync(string migrationName, CancellationToken cancellationToken)
    {
        var sql = "SELECT COUNT(*) FROM _migrations WHERE migration_name = @migrationName";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@migrationName", migrationName);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        return count > 0;
    }

    private Dictionary<string, string> GetMigrationScripts()
    {
        var migrations = new Dictionary<string, string>();
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.Contains(".Migrations.Scripts.") && name.EndsWith(".sql"))
            .OrderBy(name => name);

        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) continue;

            using var reader = new StreamReader(stream);
            var script = reader.ReadToEnd();

            // Extract migration name from resource name
            var migrationName = Path.GetFileNameWithoutExtension(resourceName.Split('.').Last());
            migrations[migrationName] = script;
        }

        return migrations;
    }
}
