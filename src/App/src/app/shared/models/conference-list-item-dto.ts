export interface ConferenceListItemDto {
    id: string;
    title: string;
    city: string;
    country: string;
    startDate: string;
    endDate: string;
    imageUrl?: string;
    isVisible: boolean;
    speakersCount: number;
    roomsCount: number;
    presentationsCount: number;
}
