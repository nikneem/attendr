import { SpeakerDto } from './speaker-dto';
import { PresentationDto } from './presentation-dto';

export interface SynchronizationSource {
    sourceType: string;
    sourceUrl: string;
}

export interface ConferenceDetailsDto {
    id: string;
    title: string;
    city: string;
    country: string;
    startDate: string;
    endDate: string;
    imageUrl?: string;
    synchronizationSource?: SynchronizationSource;
    speakers: SpeakerDto[];
    presentations: PresentationDto[];
}
