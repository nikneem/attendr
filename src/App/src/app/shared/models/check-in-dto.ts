export interface CheckInDto {
    id: string;
    groupId: string;
    conferenceId: string;
    presentationId: string;
    presentationData: CheckInPresentationDataDto;
    members: CheckedInMemberDto[];
    expiration: string;
}

export interface CheckInPresentationDataDto {
    id: string;
    title: string;
    abstract: string;
    room: string;
    startDateTime: string;
    endDateTime: string;
    speakers: CheckInPresentationSpeakerDto[];
}

export interface CheckInPresentationSpeakerDto {
    id: string;
    name: string;
    profilePictureUrl?: string;
}

export interface CheckedInMemberDto {
    id: string;
    name: string;
    profilePictureUrl?: string;
}
