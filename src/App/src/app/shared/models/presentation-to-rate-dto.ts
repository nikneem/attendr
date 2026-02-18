import { SpeakerDto } from './speaker-dto';

export interface PresentationTopicDto {
    key: string;
    name: string;
}

export interface PresentationToRateDto {
    presentationId: string;
    title: string;
    abstract: string;
    room: string;
    startDateTime: string;
    endDateTime: string;
    speakers: SpeakerDto[];
    topics: PresentationTopicDto[];
}
