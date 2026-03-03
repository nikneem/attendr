import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ConferenceSpeakerDto {
    id: string;
    name: string;
    company?: string;
    profilePictureUrl?: string;
}

export interface CreateConferenceSpeakerRequest {
    name: string;
    company?: string;
    profilePictureUrl?: string;
}

@Injectable({
    providedIn: 'root',
})
export class ConferenceSpeakersService {
    private readonly http = inject(HttpClient);

    private speakersUrl(conferenceId: string): string {
        return `${environment.apiUrl}/conferences/${conferenceId}/speakers`;
    }

    listSpeakers(conferenceId: string): Observable<ConferenceSpeakerDto[]> {
        return this.http.get<ConferenceSpeakerDto[]>(this.speakersUrl(conferenceId));
    }

    createSpeaker(conferenceId: string, request: CreateConferenceSpeakerRequest): Observable<ConferenceSpeakerDto> {
        return this.http.post<ConferenceSpeakerDto>(this.speakersUrl(conferenceId), request);
    }

    updateSpeaker(conferenceId: string, speakerId: string, request: CreateConferenceSpeakerRequest): Observable<ConferenceSpeakerDto> {
        return this.http.put<ConferenceSpeakerDto>(`${this.speakersUrl(conferenceId)}/${speakerId}`, request);
    }

    deleteSpeaker(conferenceId: string, speakerId: string): Observable<void> {
        return this.http.delete<void>(`${this.speakersUrl(conferenceId)}/${speakerId}`);
    }
}
