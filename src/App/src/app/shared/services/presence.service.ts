import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ConferencePresenceDto } from '../models/conference-presence-dto';
import { ConferenceAttendanceDto } from '../models/conference-attendance-dto';
import { ConferenceScheduleDto } from '../models/conference-schedule-dto';
import { PresentationToRateDto } from '../models/presentation-to-rate-dto';
import { CurrentConferenceDto } from '../models/current-conference-dto';

@Injectable({
    providedIn: 'root',
})
export class PresenceService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/presence`;

    getMyConferences(): Observable<ConferencePresenceDto[]> {
        return this.http.get<ConferencePresenceDto[]>(`${this.apiUrl}/my-conferences`);
    }

    getCurrentConferences(): Observable<CurrentConferenceDto[]> {
        return this.http.get<CurrentConferenceDto[]>(`${this.apiUrl}/now`);
    }

    getConferenceSchedule(conferenceId: string): Observable<ConferenceScheduleDto> {
        return this.http.get<ConferenceScheduleDto>(`${this.apiUrl}/${conferenceId}`);
    }

    updateAttendance(conferenceId: string, isAttending: boolean): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${conferenceId}/attendance`, { isAttending });
    }

    unfollowConference(conferenceId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${conferenceId}`);
    }

    getConferenceAttendance(conferenceId: string): Observable<ConferenceAttendanceDto> {
        return this.http.get<ConferenceAttendanceDto>(`${this.apiUrl}/${conferenceId}/attendance`);
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

    setPreferredPresentation(conferenceId: string, presentationId: string): Observable<void> {
        return this.http.get<void>(`${this.apiUrl}/${conferenceId}/prefer/${presentationId}`);
    }
}
