import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, KeyValue } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { NotificationPreferencesService } from '@services/notification-preferences.service';
import { NotificationSubscriptionsService } from '@services/notification-subscriptions.service';
import { PushNotificationService } from '@services/push-notification.service';
import { NotificationPreferencesDetailDto, NotificationTypePreferenceDto, ChannelPreferenceDto } from '@models/notification-preferences-detail-dto';
import { UpdateDetailedPreferencesRequest } from '@models/update-notification-preferences-request';
import { environment } from '../../../../environments/environment';

@Component({
    selector: 'attn-notification-preferences-page',
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        CardModule,
        ProgressSpinnerModule,
        ToastModule,
    ],
    templateUrl: './notification-preferences-page.component.html',
    styleUrl: './notification-preferences-page.component.scss',
})
export class NotificationPreferencesPageComponent implements OnInit {
    private readonly preferencesService = inject(NotificationPreferencesService);
    private readonly subscriptionsService = inject(NotificationSubscriptionsService);
    private readonly pushService = inject(PushNotificationService);
    private readonly messageService = inject(MessageService);
    private readonly vapidPublicKey = environment.vapidPublicKey;

    readonly channelKeys = ['InApp', 'Email', 'Push'];

    preferences = signal<NotificationPreferencesDetailDto | null>(null);
    isLoading = signal(true);
    isSaving = signal(false);
    isSendingTest = signal(false);

    ngOnInit(): void {
        this.loadPreferences();
    }

    loadPreferences(): void {
        this.isLoading.set(true);
        this.preferencesService.getDetailedPreferences().subscribe({
            next: (data) => {
                this.preferences.set(data);
                this.isLoading.set(false);
            },
            error: (error) => {
                console.error('Failed to load preferences:', error);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to load notification preferences',
                });
                this.isLoading.set(false);
            },
        });
    }

    async onChannelToggle(notificationType: NotificationTypePreferenceDto, channel: string): Promise<void> {
        const currentPref = notificationType.channelPreferences[channel];
        if (!currentPref) return;

        // Only allow toggling if channel is available
        if (!currentPref.isAvailable) return;

        // Special handling for Push channel: check for permission if enabling
        if (channel === 'Push' && !currentPref.isEnabled) {
            // User is trying to enable push notifications
            await this.handlePushChannelToggle(currentPref);
            return;
        }

        // Toggle the value
        currentPref.isEnabled = !currentPref.isEnabled;

        // Save to server
        this.savePreferences();
    }

    private async handlePushChannelToggle(currentPref: ChannelPreferenceDto): Promise<void> {
        // Check if push is supported
        if (!this.pushService.isSupported()) {
            this.messageService.add({
                severity: 'warn',
                summary: 'Not Supported',
                detail: 'Push notifications are not supported in your browser',
            });
            return;
        }

        // Check current permission
        const permission = Notification.permission;

        if (permission === 'granted') {
            // Permission already granted, enable push
            const registered = await this.registerPushSubscription();
            if (!registered) {
                return;
            }
            currentPref.isEnabled = true;
            this.savePreferences();
        } else if (permission === 'denied') {
            // Permission was previously denied
            this.messageService.add({
                severity: 'warn',
                summary: 'Permission Denied',
                detail: 'Push notification permission was denied. Please enable it in your browser settings to use push notifications.',
            });
        } else {
            // Permission not determined yet, request it
            try {
                const newPermission = await this.pushService.requestPermission();
                if (newPermission === 'granted') {
                    const registered = await this.registerPushSubscription();
                    if (!registered) {
                        return;
                    }
                    currentPref.isEnabled = true;
                    this.savePreferences();
                } else {
                    this.messageService.add({
                        severity: 'info',
                        summary: 'Permission Not Granted',
                        detail: 'Push notification permission is required to enable push notifications.',
                    });
                }
            } catch (error) {
                console.error('Error requesting push permission:', error);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to request push notification permission',
                });
            }
        }
    }

    private async registerPushSubscription(): Promise<boolean> {
        try {
            let subscription = this.pushService.subscription();

            if (!subscription) {
                if (!this.vapidPublicKey) {
                    this.messageService.add({
                        severity: 'warn',
                        summary: 'Push Setup Required',
                        detail: 'Push notifications are allowed but the VAPID public key is not configured.',
                    });
                    return false;
                }

                subscription = await this.pushService.subscribe(this.vapidPublicKey);
            }

            const subscriptionData = this.pushService.getSubscriptionData();
            if (!subscriptionData) {
                this.messageService.add({
                    severity: 'error',
                    summary: 'Push Subscription Missing',
                    detail: 'No push subscription is available to register.',
                });
                return false;
            }

            await firstValueFrom(
                this.subscriptionsService.registerSubscription({
                    endpoint: subscriptionData.endpoint,
                    p256dh: subscriptionData.keys.p256dh,
                    auth: subscriptionData.keys.auth,
                    userAgent: navigator.userAgent,
                    expirationTimeUtc: subscription?.expirationTime
                        ? new Date(subscription.expirationTime).toISOString()
                        : null,
                })
            );

            return true;
        } catch (error) {
            console.error('Failed to register push subscription:', error);
            this.messageService.add({
                severity: 'error',
                summary: 'Push Registration Failed',
                detail: 'Could not register your push subscription. Please try again.',
            });
            return false;
        }
    }

    savePreferences(): void {
        const prefs = this.preferences();
        if (!prefs) return;

        this.isSaving.set(true);

        const request: UpdateDetailedPreferencesRequest = {
            notificationTypes: prefs.notificationTypes.map((type) => ({
                typeKey: type.typeKey,
                channelPreferences: Object.fromEntries(
                    Object.entries(type.channelPreferences).map(([name, pref]) => [name, pref.isEnabled])
                ),
            })),
        };

        this.preferencesService.updateDetailedPreferences(request).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Notification preferences updated',
                });
                this.isSaving.set(false);
            },
            error: (error) => {
                console.error('Failed to update preferences:', error);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to update notification preferences',
                });
                this.isSaving.set(false);
            },
        });
    }

    getChannelDisplayName(channel: string): string {
        const names: { [key: string]: string } = {
            InApp: 'In-App',
            Email: 'Email',
            Push: 'Push',
        };
        return names[channel] || channel;
    }

    trackByTypeKey(index: number, type: NotificationTypePreferenceDto): string {
        return type.typeKey;
    }

    trackByChannelKey(index: number, key: string): string {
        return key;
    }

    sendTestNotification(): void {
        this.isSendingTest.set(true);
        this.subscriptionsService.sendTestNotification().subscribe({
            next: (response) => {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Test Sent',
                    detail: response.message,
                });
                this.isSendingTest.set(false);
            },
            error: (error) => {
                console.error('Failed to send test notification:', error);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to send test notification',
                });
                this.isSendingTest.set(false);
            },
        });
    }
}
