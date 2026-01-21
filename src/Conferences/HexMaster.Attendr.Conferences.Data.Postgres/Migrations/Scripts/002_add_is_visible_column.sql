-- Migration: Add is_visible column to conferences table
-- Description: Adds a boolean column to track whether a conference should be visible to users.

ALTER TABLE conferences 
ADD COLUMN IF NOT EXISTS is_visible BOOLEAN NOT NULL DEFAULT false;

CREATE INDEX IF NOT EXISTS idx_conferences_is_visible ON conferences(is_visible);

COMMENT ON COLUMN conferences.is_visible IS 'Indicates whether the conference is visible to users. Defaults to false (hidden).';
