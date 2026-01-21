import { SpeakerDto } from './speaker-dto';
import { TopicReferenceDto } from './topic-reference-dto';

export interface PresentationDto {
    id: string;
    title: string;
    abstract: string;
    startDateTime: string;
    endDateTime: string;
    roomName: string;
    speakers: SpeakerDto[];
    topics: TopicReferenceDto[];
}
