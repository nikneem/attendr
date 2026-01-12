# Database Migrations

This directory contains SQL migration scripts for the Conferences PostgreSQL database.

## Migration Files

Migrations are numbered sequentially and should be applied in order:

- `001_add_is_visible_column.sql` - Adds the `is_visible` column to the conferences table

## Running Migrations

### Using psql

Connect to your PostgreSQL database and run the migration:

```bash
psql -h <hostname> -U <username> -d <database> -f migrations/001_add_is_visible_column.sql
```

### Using Azure Cloud Shell

If your database is in Azure:

```bash
# Set variables
POSTGRES_HOST="your-postgres-server.postgres.database.azure.com"
POSTGRES_USER="attendradmin"
POSTGRES_DB="attendr-conferences"

# Run migration
psql "host=${POSTGRES_HOST} port=5432 dbname=${POSTGRES_DB} user=${POSTGRES_USER} sslmode=require" -f migrations/001_add_is_visible_column.sql
```

### Using Connection String

```bash
psql "postgresql://username:password@hostname:5432/database?sslmode=require" -f migrations/001_add_is_visible_column.sql
```

## Migration Checklist

When adding a new migration:

1. Create a new file with the next sequential number (e.g., `002_description.sql`)
2. Add a descriptive header comment with date and purpose
3. Use `IF NOT EXISTS` or `IF EXISTS` where appropriate to make migrations idempotent
4. Test the migration on a development database first
5. Update this README with the new migration description
6. Apply the migration to all environments (dev → staging → production)

## Rollback

If you need to rollback a migration, create a corresponding rollback script in the `rollbacks/` directory with the same number prefix.

Example: `rollbacks/001_remove_is_visible_column.sql`

## Best Practices

- Always make migrations idempotent (safe to run multiple times)
- Use transactions for complex migrations
- Add indexes for frequently queried columns
- Document any data transformations
- Test migrations on development before production
- Keep migrations small and focused on a single change
