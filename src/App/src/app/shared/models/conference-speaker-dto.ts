export interface ConferenceSpeakerDto {
    id: string;
    name: string;
    company?: string;
    profilePictureUrl?: string;
    externalId?: string;
}

export interface CreateConferenceSpeakerRequest {
    name: string;
    company?: string;
    profilePictureUrl?: string;
}

export interface UpdateConferenceSpeakerRequest {
    name: string;
    company?: string;
    profilePictureUrl?: string;
}
