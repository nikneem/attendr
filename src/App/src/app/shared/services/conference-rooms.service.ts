import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ConferenceRoomDto {
    id: string;
    name: string;
    capacity: number;
}

export interface CreateConferenceRoomRequest {
    name: string;
    capacity: number;
}

@Injectable({
    providedIn: 'root',
})
export class ConferenceRoomsService {
    private readonly http = inject(HttpClient);

    private roomsUrl(conferenceId: string): string {
        return `${environment.apiUrl}/conferences/${conferenceId}/rooms`;
    }

    listRooms(conferenceId: string): Observable<ConferenceRoomDto[]> {
        return this.http.get<ConferenceRoomDto[]>(this.roomsUrl(conferenceId));
    }

    createRoom(conferenceId: string, request: CreateConferenceRoomRequest): Observable<ConferenceRoomDto> {
        return this.http.post<ConferenceRoomDto>(this.roomsUrl(conferenceId), request);
    }

    updateRoom(conferenceId: string, roomId: string, request: CreateConferenceRoomRequest): Observable<ConferenceRoomDto> {
        return this.http.put<ConferenceRoomDto>(`${this.roomsUrl(conferenceId)}/${roomId}`, request);
    }

    deleteRoom(conferenceId: string, roomId: string): Observable<void> {
        return this.http.delete<void>(`${this.roomsUrl(conferenceId)}/${roomId}`);
    }
}
