import { SpeakerDto } from './speaker-dto';

export interface ConferenceScheduleDto {
    conferenceId: string;
    conferenceName: string;
    location: string;
    imageUrl?: string;
    startDate: string;
    endDate: string;
    isFollowing: boolean;
    isAttending: boolean;
    presentations: PresentationScheduleDto[];
}

export interface PresentationScheduleDto {
    presentationId: string;
    title: string;
    abstract: string;
    room: string;
    startDateTime: string;
    endDateTime: string;
    speakers: SpeakerDto[];
    isFavorite: boolean;
    isRecommended: boolean;
    isPreferred: boolean;
    isRated: boolean;
    isCheckedIn: boolean;
    rating?: number;
}
