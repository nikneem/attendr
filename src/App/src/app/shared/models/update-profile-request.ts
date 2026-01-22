export interface UpdateProfileRequest {
    displayName: string;
    firstName: string;
    lastName: string;
    tagLine?: string;
    isSearchable: boolean;
}
