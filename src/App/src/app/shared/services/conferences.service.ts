import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ListConferencesResult } from '../models/list-conferences-result';
import { CreateConferenceRequest } from '../models/create-conference-request.model';
import { ConferenceListItemDto } from '../models/conference-list-item-dto';

@Injectable({
    providedIn: 'root',
})
export class ConferencesService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/conferences`;

    listConferences(search?: string, pageSize?: number, pageNumber?: number): Observable<ListConferencesResult> {
        let params = new HttpParams();

        if (search) {
            params = params.set('search', search);
        }
        if (pageSize !== undefined) {
            params = params.set('pageSize', pageSize.toString());
        }
        if (pageNumber !== undefined) {
            params = params.set('pageNumber', pageNumber.toString());
        }

        return this.http.get<ListConferencesResult>(this.apiUrl, { params });
    }

    createConference(request: CreateConferenceRequest): Observable<ConferenceListItemDto> {
        return this.http.post<ConferenceListItemDto>(this.apiUrl, request);
    }
}
