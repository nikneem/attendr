-- Migration: Add is_visible column to conferences table
-- Date: 2026-01-12
-- Description: Adds a boolean column to track whether a conference should be visible to users.
--              Defaults to false for new conferences, making them hidden by default.

-- Add the is_visible column with default value false
ALTER TABLE conferences 
ADD COLUMN IF NOT EXISTS is_visible BOOLEAN NOT NULL DEFAULT false;

-- Create an index on is_visible for better query performance
CREATE INDEX IF NOT EXISTS idx_conferences_is_visible ON conferences(is_visible);

-- Optional: Update existing conferences to be visible by default
-- Uncomment the following line if you want existing conferences to be visible
-- UPDATE conferences SET is_visible = true WHERE is_visible = false;

-- Add comment to document the column
COMMENT ON COLUMN conferences.is_visible IS 'Indicates whether the conference is visible to users. Defaults to false (hidden).';
