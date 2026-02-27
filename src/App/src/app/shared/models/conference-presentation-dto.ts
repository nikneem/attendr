import { ConferenceSpeakerDto } from './conference-speaker-dto';

export interface ConferencePresentationDto {
    id: string;
    title: string;
    abstract: string;
    startDateTime: string;
    endDateTime: string;
    roomId: string;
    roomName: string;
    speakerIds: string[];
    speakers: ConferenceSpeakerDto[];
    externalId?: string;
}

export interface CreateConferencePresentationRequest {
    title: string;
    abstract: string;
    startDateTime: string;
    endDateTime: string;
    roomId: string;
    speakerIds: string[];
}

export interface UpdateConferencePresentationRequest {
    title: string;
    abstract: string;
    startDateTime: string;
    endDateTime: string;
    roomId: string;
    speakerIds: string[];
}
