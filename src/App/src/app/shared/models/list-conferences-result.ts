import { ConferenceListItemDto } from './conference-list-item-dto';

export interface ListConferencesResult {
    conferences: ConferenceListItemDto[];
    totalCount: number;
    pageSize: number;
    pageNumber: number;
}
