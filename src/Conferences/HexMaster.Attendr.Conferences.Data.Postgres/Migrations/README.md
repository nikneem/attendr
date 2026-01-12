# Automatic Database Migrations

The Conferences API now automatically applies database migrations on startup.

## How It Works

1. **Embedded Resources**: Migration SQL scripts are embedded into the assembly from the `Migrations/Scripts` folder
2. **Migration Tracking**: A `_migrations` table tracks which migrations have been applied
3. **Startup Execution**: The `MigrationHostedService` runs during application startup
4. **Idempotent**: Migrations use `IF NOT EXISTS` clauses and are only applied once
5. **Transactional**: Each migration runs in a transaction - if it fails, changes are rolled back

## Adding New Migrations

1. Create a new SQL file in `Migrations/Scripts/` with a sequential number prefix:
   ```
   002_add_new_column.sql
   003_create_index.sql
   ```

2. Write idempotent SQL (safe to run multiple times):
   ```sql
   ALTER TABLE conferences 
   ADD COLUMN IF NOT EXISTS new_column VARCHAR(255);
   ```

3. The file will automatically be included as an embedded resource (via `*.sql` pattern in .csproj)

4. On next startup, the migration will be detected and applied

## Migration File Naming Convention

Format: `NNN_description.sql`

- `NNN`: Three-digit sequential number (001, 002, 003...)
- `description`: Brief description using snake_case or kebab-case
- Extension: `.sql`

Examples:
- `001_add_is_visible_column.sql`
- `002_create_speakers_index.sql`
- `003_add_conference_tags_table.sql`

## Benefits

✅ **No Manual Steps**: Migrations run automatically when the API starts
✅ **Version Control**: Migration scripts are checked into source control
✅ **Consistent**: All environments get the same migrations in the same order
✅ **Safe**: Transactions ensure all-or-nothing application
✅ **Tracked**: The `_migrations` table shows what's been applied

## Viewing Applied Migrations

Query the migrations table:

```sql
SELECT * FROM _migrations ORDER BY applied_at;
```

## Development Workflow

1. Developer adds a new migration SQL file
2. Commits to source control
3. CI/CD deploys the new version
4. On startup, the API automatically applies the new migration
5. Application is ready to use with updated schema

## Troubleshooting

### Migration Failed

Check the application logs for details. The migration will be rolled back and the application will continue starting (though it may not work correctly with the old schema).

### Re-running a Failed Migration

If a migration fails, fix the SQL and delete the record from `_migrations`:

```sql
DELETE FROM _migrations WHERE migration_name = '001_add_is_visible_column';
```

Then restart the API to retry.

### Disable Automatic Migrations

Comment out this line in `Program.cs`:

```csharp
// builder.Services.AddDatabaseMigrations();
```

## Best Practices

1. **Test Locally First**: Always test migrations on a local database before deploying
2. **Keep Migrations Small**: Each migration should do one thing
3. **Use Transactions**: Complex migrations should be wrapped in explicit transactions
4. **Make Idempotent**: Use `IF NOT EXISTS`, `IF EXISTS`, etc.
5. **Don't Modify Old Migrations**: Once applied in any environment, treat migrations as immutable
6. **Add Comments**: Document what each migration does and why

## Architecture

```
Conferences.Api (Program.cs)
    ↓ calls
ServiceCollectionExtensions.AddDatabaseMigrations()
    ↓ registers
MigrationHostedService (IHostedService)
    ↓ executes on startup
DatabaseMigrationRunner
    ↓ reads
Embedded SQL Scripts (Migrations/Scripts/*.sql)
    ↓ applies to
PostgreSQL Database
```
