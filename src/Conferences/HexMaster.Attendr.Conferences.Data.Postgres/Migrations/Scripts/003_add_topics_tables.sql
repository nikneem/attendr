-- Migration: Add topics and presentation_topics tables
-- Description: Creates topics table to categorize presentations and junction table to link presentations to topics

-- Add is_analysed column to presentations table
ALTER TABLE presentations
ADD COLUMN IF NOT EXISTS is_analysed BOOLEAN NOT NULL DEFAULT false;

-- Create topics table
CREATE TABLE IF NOT EXISTS topics (
    id UUID PRIMARY KEY,
    key VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(500) NOT NULL,
    is_visible BOOLEAN NOT NULL DEFAULT true,
    created_on TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create presentation_topics junction table
CREATE TABLE IF NOT EXISTS presentation_topics (
    id UUID PRIMARY KEY,
    presentation_id UUID NOT NULL,
    topic_id UUID NOT NULL,
    FOREIGN KEY (presentation_id) REFERENCES presentations(id) ON DELETE CASCADE,
    FOREIGN KEY (topic_id) REFERENCES topics(id) ON DELETE CASCADE,
    UNIQUE(presentation_id, topic_id)
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_topics_key ON topics(key);
CREATE INDEX IF NOT EXISTS idx_topics_is_visible ON topics(is_visible);
CREATE INDEX IF NOT EXISTS idx_presentations_is_analysed ON presentations(is_analysed);
CREATE INDEX IF NOT EXISTS idx_presentation_topics_presentation_id ON presentation_topics(presentation_id);
CREATE INDEX IF NOT EXISTS idx_presentation_topics_topic_id ON presentation_topics(topic_id);
