export interface ConferenceScheduleNowDto {
    previous: ScheduledPresentationDto[];
    now: ScheduledPresentationDto[];
    next: ScheduledPresentationDto[];
}

export interface ScheduledPresentationDto {
    presentationId: string;
    title: string;
    abstract: string;
    room: string;
    startDateTime: string;
    endDateTime: string;
    speakers: ScheduledSpeakerDto[];
    isPreferred: boolean;
}

export interface ScheduledSpeakerDto {
    speakerId: string;
    name: string;
    profilePictureUrl?: string;
}
