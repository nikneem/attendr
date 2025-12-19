import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ListGroupsResult } from '../models/list-groups-result';

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
}
