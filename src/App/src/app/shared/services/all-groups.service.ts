import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ListGroupsResult } from '../models/list-groups-result';
import { GroupDetailsDto } from '../models/group-details-dto';

@Injectable({
    providedIn: 'root',
})
export class AllGroupsService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/groups`;

    listGroups(searchQuery?: string, pageSize?: number, pageNumber?: number): Observable<ListGroupsResult> {
        let params = new HttpParams();

        if (searchQuery) {
            params = params.set('searchQuery', searchQuery);
        }
        if (pageSize !== undefined) {
            params = params.set('pageSize', pageSize.toString());
        }
        if (pageNumber !== undefined) {
            params = params.set('pageNumber', pageNumber.toString());
        }

        return this.http.get<ListGroupsResult>(this.apiUrl, { params });
    }

    getGroupDetails(id: string): Observable<GroupDetailsDto> {
        return this.http.get<GroupDetailsDto>(`${this.apiUrl}/${id}`);
    }

    joinGroup(groupId: string): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${groupId}/members`, {});
    }

    removeMember(groupId: string, memberId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${groupId}/members/${memberId}`);
    }

    approveJoinRequest(groupId: string, profileId: string): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${groupId}/join-requests/${profileId}/approve`, {});
    }

    denyJoinRequest(groupId: string, profileId: string): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${groupId}/join-requests/${profileId}/deny`, {});
    }

    followConference(groupId: string, conferenceId: string): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${groupId}/conferences`, { conferenceId });
    }

    unfollowConference(groupId: string, conferenceId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${groupId}/conferences/${conferenceId}`);
    }
}
