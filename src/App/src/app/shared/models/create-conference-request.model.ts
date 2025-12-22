export interface SynchronizationSource {
    sourceType: string;
    sourceUrl: string;
}

export interface CreateConferenceRequest {
    title: string;
    city?: string;
    country?: string;
    imageUrl?: string;
    startDate: string;
    endDate: string;
    synchronizationSource?: SynchronizationSource;
}
