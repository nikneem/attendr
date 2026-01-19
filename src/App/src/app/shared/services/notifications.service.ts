import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, interval } from 'rxjs';
import { tap, startWith, switchMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { NotificationDto } from '../models/notification-dto';

@Injectable({
    providedIn: 'root',
})
export class NotificationsService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/notifications`;

    // Signal for reactive unread count
    readonly unreadCount = signal<number>(0);

    getNotifications(includeRead: boolean = true): Observable<NotificationDto[]> {
        return this.http.get<NotificationDto[]>(`${this.apiUrl}?includeRead=${includeRead}`);
    }

    getUnreadCount(): Observable<number> {
        return this.http.get<number>(`${this.apiUrl}/unread/count`).pipe(
            tap(count => this.unreadCount.set(count))
        );
    }

    markAsRead(notificationId: string): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${notificationId}/read`, {});
    }

    markAllAsRead(): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/read-all`, {});
    }

    deleteNotification(notificationId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${notificationId}`);
    }

    /**
     * Creates an observable that polls for notifications at the specified interval
     * @param intervalMs Polling interval in milliseconds (default: 180000 = 3 minutes)
     */
    pollNotifications(intervalMs: number = 180000): Observable<NotificationDto[]> {
        return interval(intervalMs).pipe(
            startWith(0), // Emit immediately on subscription
            switchMap(() => this.getNotifications(false)) // Only get unread
        );
    }

    /**
     * Creates an observable that polls for unread count at the specified interval
     * @param intervalMs Polling interval in milliseconds (default: 180000 = 3 minutes)
     */
    pollUnreadCount(intervalMs: number = 180000): Observable<number> {
        return interval(intervalMs).pipe(
            startWith(0), // Emit immediately on subscription
            switchMap(() => this.getUnreadCount())
        );
    }
}
