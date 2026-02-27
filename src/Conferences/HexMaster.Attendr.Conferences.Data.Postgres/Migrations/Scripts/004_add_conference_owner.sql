-- Migration: AddConferenceOwner
-- Description: Adds created_by_profile_id column to conferences table to track ownership.
-- The column is nullable to remain backwards-compatible with conferences created before this change.

ALTER TABLE conferences
ADD COLUMN IF NOT EXISTS created_by_profile_id UUID NULL;

CREATE INDEX IF NOT EXISTS idx_conferences_created_by_profile_id ON conferences(created_by_profile_id);

COMMENT ON COLUMN conferences.created_by_profile_id IS 'The profile ID of the user who created this conference. NULL for conferences created before ownership tracking was introduced.';
