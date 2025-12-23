import { SpeakerDto } from './speaker-dto';

export interface PresentationDto {
    id: string;
    title: string;
    abstract: string;
    startDateTime: string;
    endDateTime: string;
    roomName: string;
    speakers: SpeakerDto[];
}
