import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ConferenceSpeakerDto } from './conference-speakers.service';

export interface ConferencePresentationDto {
    id: string;
    title: string;
    abstract: string;
    startDateTime: string;
    endDateTime: string;
    roomId: string;
    roomName: string;
    speakerIds: string[];
    speakers: ConferenceSpeakerDto[];
}

export interface CreateConferencePresentationRequest {
    title: string;
    abstract: string;
    startDateTime: string;
    endDateTime: string;
    roomId: string;
    speakerIds: string[];
}

@Injectable({
    providedIn: 'root',
})
export class ConferencePresentationsService {
    private readonly http = inject(HttpClient);

    private presentationsUrl(conferenceId: string): string {
        return `${environment.apiUrl}/conferences/${conferenceId}/presentations`;
    }

    listPresentations(conferenceId: string): Observable<ConferencePresentationDto[]> {
        return this.http.get<ConferencePresentationDto[]>(this.presentationsUrl(conferenceId));
    }

    createPresentation(conferenceId: string, request: CreateConferencePresentationRequest): Observable<ConferencePresentationDto> {
        return this.http.post<ConferencePresentationDto>(this.presentationsUrl(conferenceId), request);
    }

    updatePresentation(conferenceId: string, presentationId: string, request: CreateConferencePresentationRequest): Observable<ConferencePresentationDto> {
        return this.http.put<ConferencePresentationDto>(`${this.presentationsUrl(conferenceId)}/${presentationId}`, request);
    }

    deletePresentation(conferenceId: string, presentationId: string): Observable<void> {
        return this.http.delete<void>(`${this.presentationsUrl(conferenceId)}/${presentationId}`);
    }
}
