export interface GroupMemberDto {
    id: string;
    name: string;
    role: number; // 0 = Owner, 1 = Manager, 2 = Member
}

export interface GroupInvitationDto {
    id: string;
    name: string;
    expirationDate: string;
}

export interface GroupJoinRequestDto {
    id: string;
    name: string;
    requestedAt: string;
}

export interface GroupDetailsDto {
    id: string;
    name: string;
    memberCount: number;
    isMember: boolean;
    isPublic: boolean;
    currentMemberRole: number | null; // 0 = Owner, 1 = Manager, 2 = Member, null if not a member
    members: GroupMemberDto[];
    invitations: GroupInvitationDto[];
    joinRequests: GroupJoinRequestDto[];
}
