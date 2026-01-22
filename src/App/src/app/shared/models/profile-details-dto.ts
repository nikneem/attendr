export interface ProfileDetailsDto {
    profileId: string;
    displayName: string;
    firstName?: string;
    lastName?: string;
    email: string;
    profilePictureUrl?: string;
    tagLine?: string;
    isSearchable: boolean;
}
