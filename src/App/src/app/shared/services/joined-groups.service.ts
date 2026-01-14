import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MyGroupDto } from '../models/my-group-dto';
import { CreateGroupRequest } from '../models/create-group-request';
import { CreateGroupResult } from '../models/create-group-result';
import { UpdateGroupRequest } from '../models/update-group-request';
import { UpdateGroupResult } from '../models/update-group-result';
import { CheckInDto } from '../models/check-in-dto';

@Injectable({
    providedIn: 'root',
})
export class JoinedGroupsService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/groups`;

    getMyGroups(): Observable<MyGroupDto[]> {
        return this.http.get<MyGroupDto[]>(`${this.apiUrl}/my-groups`);
    }

    createGroup(request: CreateGroupRequest): Observable<CreateGroupResult> {
        return this.http.post<CreateGroupResult>(this.apiUrl, request);
    }

    updateGroup(groupId: string, request: UpdateGroupRequest): Observable<UpdateGroupResult> {
        return this.http.put<UpdateGroupResult>(`${this.apiUrl}/${groupId}`, request);
    }

    getGroupCheckIns(groupId: string): Observable<CheckInDto[]> {
        return this.http.get<CheckInDto[]>(`${this.apiUrl}/${groupId}/checkins`);
    }
}
