export interface CreateGroupResult {
    id: string;
    name: string;
    members: GroupMemberDto[];
}

export interface GroupMemberDto {
    id: string;
    name: string;
    role: number;
}
