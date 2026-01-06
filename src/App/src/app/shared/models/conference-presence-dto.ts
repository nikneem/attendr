export interface ConferencePresenceDto {
    conferenceId: string;
    conferenceName: string;
    location: string;
    imageUrl?: string;
    startDate: string;
    endDate: string;
    isFollowing: boolean;
    isAttending: boolean;
}
