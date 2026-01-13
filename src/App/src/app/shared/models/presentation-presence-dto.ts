import { SpeakerDto } from './speaker-dto';

export interface PresentationPresenceDto {
    id: string;
    title: string;
    abstract: string;
    startDateTime: string;
    endDateTime: string;
    roomName: string;
    speakers: SpeakerDto[];
    isFavorite: boolean;
    isRecommended: boolean;
    isPreferred: boolean;
}
