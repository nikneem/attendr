import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NotificationPreferencesDetailDto } from '../models/notification-preferences-detail-dto';
import { UpdateDetailedPreferencesRequest } from '../models/update-notification-preferences-request';

@Injectable({
    providedIn: 'root',
})
export class NotificationPreferencesService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/notifications/preferences`;

    getDetailedPreferences(): Observable<NotificationPreferencesDetailDto> {
        return this.http.get<NotificationPreferencesDetailDto>(`${this.apiUrl}/detailed`);
    }

    updateDetailedPreferences(request: UpdateDetailedPreferencesRequest): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/detailed`, request);
    }
}
