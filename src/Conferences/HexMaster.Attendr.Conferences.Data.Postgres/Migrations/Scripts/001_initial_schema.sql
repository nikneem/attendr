-- Migration: Create initial database schema
-- Description: Creates the initial schema with all tables for the Conferences service

-- Create conferences table
CREATE TABLE IF NOT EXISTS conferences (
    id UUID PRIMARY KEY,
    title VARCHAR(500) NOT NULL,
    city VARCHAR(200) NOT NULL,
    country VARCHAR(200) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    image_url VARCHAR(1000),
    sync_source_type INTEGER,
    sync_source_location_or_api_key VARCHAR(1000)
);

-- Create rooms table
CREATE TABLE IF NOT EXISTS rooms (
    id UUID PRIMARY KEY,
    conference_id UUID NOT NULL,
    name VARCHAR(200) NOT NULL,
    capacity INTEGER NOT NULL,
    external_id VARCHAR(200),
    FOREIGN KEY (conference_id) REFERENCES conferences(id) ON DELETE CASCADE
);

-- Create speakers table
CREATE TABLE IF NOT EXISTS speakers (
    id UUID PRIMARY KEY,
    conference_id UUID NOT NULL,
    name VARCHAR(500) NOT NULL,
    company VARCHAR(500),
    profile_picture_url VARCHAR(1000),
    external_id VARCHAR(200),
    FOREIGN KEY (conference_id) REFERENCES conferences(id) ON DELETE CASCADE
);

-- Create presentations table
CREATE TABLE IF NOT EXISTS presentations (
    id UUID PRIMARY KEY,
    conference_id UUID NOT NULL,
    room_id UUID NOT NULL,
    title VARCHAR(500) NOT NULL,
    abstract TEXT NOT NULL,
    start_date_time TIMESTAMP NOT NULL,
    end_date_time TIMESTAMP NOT NULL,
    external_id VARCHAR(200),
    FOREIGN KEY (conference_id) REFERENCES conferences(id) ON DELETE CASCADE,
    FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE CASCADE
);

-- Create presentation_speakers junction table
CREATE TABLE IF NOT EXISTS presentation_speakers (
    presentation_id UUID NOT NULL,
    speaker_id UUID NOT NULL,
    PRIMARY KEY (presentation_id, speaker_id),
    FOREIGN KEY (presentation_id) REFERENCES presentations(id) ON DELETE CASCADE,
    FOREIGN KEY (speaker_id) REFERENCES speakers(id) ON DELETE CASCADE
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_conferences_end_date ON conferences(end_date);
CREATE INDEX IF NOT EXISTS idx_conferences_start_date ON conferences(start_date);
CREATE INDEX IF NOT EXISTS idx_rooms_conference_id ON rooms(conference_id);
CREATE INDEX IF NOT EXISTS idx_speakers_conference_id ON speakers(conference_id);
CREATE INDEX IF NOT EXISTS idx_presentations_conference_id ON presentations(conference_id);
CREATE INDEX IF NOT EXISTS idx_presentations_start_date_time ON presentations(start_date_time);
CREATE INDEX IF NOT EXISTS idx_presentation_speakers_speaker_id ON presentation_speakers(speaker_id);
