import { Component, ViewChild, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Popover, PopoverModule } from 'primeng/popover';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { NotificationsService } from '@services/notifications.service';
import { NotificationDto } from '@models/notification-dto';
import { Subscription } from 'rxjs';

@Component({
    selector: 'attn-notifications-button',
    imports: [PopoverModule, BadgeModule, ButtonModule, ProgressSpinnerModule, TooltipModule],
    templateUrl: './notifications-button.component.html',
    styleUrl: './notifications-button.component.scss',
})
export class NotificationsButtonComponent implements OnInit, OnDestroy {
    @ViewChild('notificationsPanel') notificationsPanel?: Popover;

    private readonly notificationsService = inject(NotificationsService);
    private readonly router = inject(Router);
    private pollingSubscription?: Subscription;

    protected readonly notifications = signal<NotificationDto[]>([]);
    protected readonly unreadCount = signal<number>(0);
    protected readonly loading = signal<boolean>(false);

    ngOnInit(): void {
        // Start polling for unread count every 3 minutes (180000ms)
        this.pollingSubscription = this.notificationsService.pollUnreadCount(180000).subscribe({
            next: (count) => {
                this.unreadCount.set(count);
            },
            error: (error) => {
                console.error('Error polling unread count:', error);
            }
        });
    }

    ngOnDestroy(): void {
        if (this.pollingSubscription) {
            this.pollingSubscription.unsubscribe();
        }
    }

    toggleNotifications(event: Event): void {
        this.loadNotifications();
        this.notificationsPanel?.toggle(event);
    }

    private loadNotifications(): void {
        this.loading.set(true);
        this.notificationsService.getNotifications(false).subscribe({
            next: (notifications) => {
                this.notifications.set(notifications);
                this.loading.set(false);
            },
            error: (error) => {
                console.error('Error loading notifications:', error);
                this.loading.set(false);
            }
        });
    }

    handleNotificationClick(notification: NotificationDto): void {
        // Mark as read if not already
        if (!notification.isRead) {
            this.markAsRead(notification.id);
        }

        // Navigate to URL if provided
        if (notification.url) {
            this.notificationsPanel?.hide();
            this.router.navigateByUrl(notification.url);
        }
    }

    markAsRead(notificationId: string): void {
        this.notificationsService.markAsRead(notificationId).subscribe({
            next: () => {
                // Update local state
                this.notifications.update(notifications =>
                    notifications.map(n =>
                        n.id === notificationId ? { ...n, isRead: true, readAt: new Date() } : n
                    )
                );
                this.unreadCount.update(count => Math.max(0, count - 1));
            },
            error: (error) => {
                console.error('Error marking notification as read:', error);
            }
        });
    }

    markAllAsRead(): void {
        this.notificationsService.markAllAsRead().subscribe({
            next: () => {
                // Update local state
                this.notifications.update(notifications =>
                    notifications.map(n => ({ ...n, isRead: true, readAt: new Date() }))
                );
                this.unreadCount.set(0);
            },
            error: (error) => {
                console.error('Error marking all notifications as read:', error);
            }
        });
    }

    deleteNotification(notificationId: string): void {
        this.notificationsService.deleteNotification(notificationId).subscribe({
            next: () => {
                // Remove from local state
                const wasUnread = !this.notifications().find(n => n.id === notificationId)?.isRead;
                this.notifications.update(notifications =>
                    notifications.filter(n => n.id !== notificationId)
                );
                if (wasUnread) {
                    this.unreadCount.update(count => Math.max(0, count - 1));
                }
            },
            error: (error) => {
                console.error('Error deleting notification:', error);
            }
        });
    }

    formatTime(date: Date): string {
        const now = new Date();
        const notificationDate = new Date(date);
        const diffMs = now.getTime() - notificationDate.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;

        return notificationDate.toLocaleDateString();
    }
}
