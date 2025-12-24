import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ConferencePresenceDto } from '../models/conference-presence-dto';

@Injectable({
    providedIn: 'root',
})
export class PresenceService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/presence`;

    getMyConferences(): Observable<ConferencePresenceDto[]> {
        return this.http.get<ConferencePresenceDto[]>(`${this.apiUrl}/my-conferences`);
    }
}
