import { GroupListItemDto } from './group-list-item-dto';

export interface ListGroupsResult {
    groups: GroupListItemDto[];
    totalCount: number;
    pageSize: number;
    pageNumber: number;
}
