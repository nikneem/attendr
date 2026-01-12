-- Rollback: Remove is_visible column from conferences table
-- Date: 2026-01-12
-- Description: Removes the is_visible column if the migration needs to be rolled back

-- Drop the index first
DROP INDEX IF EXISTS idx_conferences_is_visible;

-- Remove the column
ALTER TABLE conferences 
DROP COLUMN IF EXISTS is_visible;
