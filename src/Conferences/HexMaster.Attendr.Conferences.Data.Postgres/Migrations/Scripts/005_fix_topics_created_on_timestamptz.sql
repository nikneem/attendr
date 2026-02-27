-- Migration: Fix topics.created_on column to use TIMESTAMPTZ
-- Description: Changes created_on in the topics table from TIMESTAMP (no timezone)
--              to TIMESTAMPTZ (with time zone) to align with the date/time conventions spec.
--              Existing values are treated as UTC (which they always were).

ALTER TABLE topics
    ALTER COLUMN created_on TYPE TIMESTAMPTZ
    USING created_on AT TIME ZONE 'UTC';
