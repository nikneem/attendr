import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ListConferencesResult } from '../models/list-conferences-result';
import { CreateConferenceRequest } from '../models/create-conference-request.model';
import { ConferenceListItemDto } from '../models/conference-list-item-dto';
import { ConferenceDetailsDto } from '../models/conference-details-dto';
import { ConferenceMetricsDto } from '../models/conference-metrics-dto';
import { ConferenceSpeakerDto, CreateConferenceSpeakerRequest, UpdateConferenceSpeakerRequest } from '../models/conference-speaker-dto';
import { ConferenceRoomDto, CreateConferenceRoomRequest, UpdateConferenceRoomRequest } from '../models/conference-room-dto';
import { ConferencePresentationDto, CreateConferencePresentationRequest, UpdateConferencePresentationRequest } from '../models/conference-presentation-dto';

@Injectable({
    providedIn: 'root',
})
export class ConferencesService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/conferences`;

    listConferences(search?: string, pageSize?: number, pageNumber?: number, showHidden?: boolean): Observable<ListConferencesResult> {
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
        if (showHidden === true) {
            params = params.set('showHidden', 'true');
        }

        return this.http.get<ListConferencesResult>(this.apiUrl, { params });
    }

    getConference(id: string): Observable<ConferenceDetailsDto> {
        return this.http.get<ConferenceDetailsDto>(`${this.apiUrl}/${id}`);
    }

    createConference(request: CreateConferenceRequest): Observable<ConferenceListItemDto> {
        return this.http.post<ConferenceListItemDto>(this.apiUrl, request);
    }

    updateConference(id: string, request: CreateConferenceRequest): Observable<ConferenceDetailsDto> {
        return this.http.put<ConferenceDetailsDto>(`${this.apiUrl}/${id}`, request);
    }

    followConference(id: string): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${id}/follow`, {});
    }

    deleteConference(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    getMetrics(): Observable<ConferenceMetricsDto> {
        return this.http.get<ConferenceMetricsDto>(`${this.apiUrl}/metrics`);
    }

    // Speakers
    listSpeakers(conferenceId: string): Observable<ConferenceSpeakerDto[]> {
        return this.http.get<ConferenceSpeakerDto[]>(`${this.apiUrl}/${conferenceId}/speakers`);
    }
    createSpeaker(conferenceId: string, request: CreateConferenceSpeakerRequest): Observable<ConferenceSpeakerDto> {
        return this.http.post<ConferenceSpeakerDto>(`${this.apiUrl}/${conferenceId}/speakers`, request);
    }
    updateSpeaker(conferenceId: string, speakerId: string, request: UpdateConferenceSpeakerRequest): Observable<ConferenceSpeakerDto> {
        return this.http.put<ConferenceSpeakerDto>(`${this.apiUrl}/${conferenceId}/speakers/${speakerId}`, request);
    }
    deleteSpeaker(conferenceId: string, speakerId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${conferenceId}/speakers/${speakerId}`);
    }

    // Rooms
    listRooms(conferenceId: string): Observable<ConferenceRoomDto[]> {
        return this.http.get<ConferenceRoomDto[]>(`${this.apiUrl}/${conferenceId}/rooms`);
    }
    createRoom(conferenceId: string, request: CreateConferenceRoomRequest): Observable<ConferenceRoomDto> {
        return this.http.post<ConferenceRoomDto>(`${this.apiUrl}/${conferenceId}/rooms`, request);
    }
    updateRoom(conferenceId: string, roomId: string, request: UpdateConferenceRoomRequest): Observable<ConferenceRoomDto> {
        return this.http.put<ConferenceRoomDto>(`${this.apiUrl}/${conferenceId}/rooms/${roomId}`, request);
    }
    deleteRoom(conferenceId: string, roomId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${conferenceId}/rooms/${roomId}`);
    }

    // Presentations
    listPresentations(conferenceId: string): Observable<ConferencePresentationDto[]> {
        return this.http.get<ConferencePresentationDto[]>(`${this.apiUrl}/${conferenceId}/presentations`);
    }
    createPresentation(conferenceId: string, request: CreateConferencePresentationRequest): Observable<ConferencePresentationDto> {
        return this.http.post<ConferencePresentationDto>(`${this.apiUrl}/${conferenceId}/presentations`, request);
    }
    updatePresentation(conferenceId: string, presentationId: string, request: UpdateConferencePresentationRequest): Observable<ConferencePresentationDto> {
        return this.http.put<ConferencePresentationDto>(`${this.apiUrl}/${conferenceId}/presentations/${presentationId}`, request);
    }
    deletePresentation(conferenceId: string, presentationId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${conferenceId}/presentations/${presentationId}`);
    }
}
