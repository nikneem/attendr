import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RegisterPushSubscriptionRequest } from '../models/register-push-subscription-request';

export interface TestNotificationResponse {
    sentCount: number;
    message: string;
}

@Injectable({
    providedIn: 'root',
})
export class NotificationSubscriptionsService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/notifications/subscriptions`;

    registerSubscription(request: RegisterPushSubscriptionRequest): Observable<void> {
        return this.http.post<void>(this.apiUrl, request);
    }

    unsubscribe(endpoint: string): Observable<void> {
        return this.http.delete<void>(this.apiUrl, {
            body: { endpoint }
        });
    }

    sendTestNotification(): Observable<TestNotificationResponse> {
        return this.http.get<TestNotificationResponse>(`${this.apiUrl}/test`);
    }
}
