import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ConferencePresenceDto } from '../models/conference-presence-dto';
import { PresentationToRateDto } from '../models/presentation-to-rate-dto';

@Injectable({
    providedIn: 'root',
})
export class PresenceService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/presence`;

    getMyConferences(): Observable<ConferencePresenceDto[]> {
        return this.http.get<ConferencePresenceDto[]>(`${this.apiUrl}/my-conferences`);
    }

    updateAttendance(conferenceId: string, isAttending: boolean): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${conferenceId}/attendance`, { isAttending });
    }

    unfollowConference(conferenceId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${conferenceId}`);
    }

    getPresentationToRate(conferenceId: string, index: number): Observable<PresentationToRateDto> {
        return this.http.get<PresentationToRateDto>(`${this.apiUrl}/${conferenceId}/rate?index=${index}`);
    }

    ratePresentation(conferenceId: string, presentationId: string, rating: number | null, isFavorite: boolean): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${conferenceId}/rate/${presentationId}`, {
            rating,
            isFavorite
        });
    }
}
