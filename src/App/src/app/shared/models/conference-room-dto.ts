export interface ConferenceRoomDto {
    id: string;
    name: string;
    capacity: number;
    externalId?: string;
}

export interface CreateConferenceRoomRequest {
    name: string;
    capacity: number;
}

export interface UpdateConferenceRoomRequest {
    name: string;
    capacity: number;
}
