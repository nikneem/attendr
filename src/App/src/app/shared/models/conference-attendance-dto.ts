export interface ConferenceAttendanceDto {
    conferenceId: string;
    isFollowing: boolean;
    isAttending: boolean;
    favoritePresentationIds: string[];
}
