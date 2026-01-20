import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule, KeyValue } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { NotificationPreferencesService } from '@services/notification-preferences.service';
import { NotificationSubscriptionsService } from '@services/notification-subscriptions.service';
import { PushNotificationService } from '@services/push-notification.service';
import { NotificationPreferencesDetailDto, NotificationTypePreferenceDto, ChannelPreferenceDto } from '@models/notification-preferences-detail-dto';
import { UpdateDetailedPreferencesRequest } from '@models/update-notification-preferences-request';
import { environment } from '../../../../environments/environment';

interface BeforeInstallPromptEvent extends Event {
    prompt: () => Promise<void>;
    userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

@Component({
    selector: 'attn-notification-preferences-page',
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        CardModule,
        ProgressSpinnerModule,
        MessageModule,
    ],
    templateUrl: './notification-preferences-page.component.html',
    styleUrl: './notification-preferences-page.component.scss',
})
export class NotificationPreferencesPageComponent implements OnInit, OnDestroy {
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

    // PWA Detection signals
    isMobileDevice = signal(false);
    isPwaInstalled = signal(false);
    pushNotificationsAllowed = signal(false);
    showPwaInstallBanner = signal(false);
    showPermissionBanner = signal(false);
    isInstallingApp = signal(false);
    isRequestingPermission = signal(false);

    // Track if the current browser subscription is registered on backend
    private isSubscriptionRegistered = false;
    private registeredEndpoint: string | null = null;
    
    private deferredPrompt: BeforeInstallPromptEvent | null = null;

    ngOnInit(): void {
        this.detectPwaStatus();
        this.listenForInstallPrompt();
        this.loadPreferences();
    }

    ngOnDestroy(): void {
        // Cleanup event listeners
        window.removeEventListener('beforeinstallprompt', this.handleBeforeInstallPrompt);
    }

    private detectPwaStatus(): void {
        // Detect mobile device
        const userAgent = navigator.userAgent.toLowerCase();
        const isMobile = /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini/.test(userAgent);
        this.isMobileDevice.set(isMobile);

        // Detect if app is already installed as PWA
        const isPwa = window.matchMedia('(display-mode: standalone)').matches
            || (window.navigator as any).standalone === true;
        this.isPwaInstalled.set(isPwa);

        // Detect if push notifications permission is granted
        const pushAllowed = 'Notification' in window && Notification.permission === 'granted';
        this.pushNotificationsAllowed.set(pushAllowed);
    }

    private listenForInstallPrompt(): void {
        window.addEventListener('beforeinstallprompt', this.handleBeforeInstallPrompt);
    }

    private handleBeforeInstallPrompt = (event: Event) => {
        const beforeInstallPromptEvent = event as BeforeInstallPromptEvent;
        // Prevent the mini-infobar from appearing on mobile
        beforeInstallPromptEvent.preventDefault();
        // Stash the event for later use
        this.deferredPrompt = beforeInstallPromptEvent;

        // Show the install banner if:
        // - On mobile
        // - App is not already installed
        // - Install prompt is available
        if (this.isMobileDevice() && !this.isPwaInstalled()) {
            this.showPwaInstallBanner.set(true);
        }
    };

    async installApp(): Promise<void> {
        if (!this.deferredPrompt) {
            this.messageService.add({
                severity: 'warn',
                summary: 'Installation Unavailable',
                detail: 'The install prompt is not available. Please try again later.',
            });
            return;
        }

        this.isInstallingApp.set(true);

        try {
            // Trigger the install prompt
            await this.deferredPrompt.prompt();

            // Wait for user choice
            const choiceResult = await this.deferredPrompt.userChoice;

            if (choiceResult.outcome === 'accepted') {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Installation Started',
                    detail: 'The app is being installed. You can now use Attendr as a standalone app with better performance and offline support.',
                });
                this.showPwaInstallBanner.set(false);
                this.isPwaInstalled.set(true);
            } else {
                console.log('User dismissed the install prompt');
            }
        } catch (error) {
            console.error('Error during app installation:', error);
            this.messageService.add({
                severity: 'error',
                summary: 'Installation Failed',
                detail: 'Failed to install the app. Please try again.',
            });
        } finally {
            this.isInstallingApp.set(false);
            // Clear the deferred prompt
            this.deferredPrompt = null;
        }
    }

    dismissInstallBanner(): void {
        this.showPwaInstallBanner.set(false);
        this.deferredPrompt = null;
    }

    dismissPermissionBanner(): void {
        this.showPermissionBanner.set(false);
    }

    private hasAnyPushNotificationsEnabled(): boolean {
        const prefs = this.preferences();
        if (!prefs) return false;

        return prefs.notificationTypes.some((type) => {
            const pushPref = type.channelPreferences['Push'];
            return pushPref?.isAvailable && pushPref?.isEnabled;
        });
    }

    private updatePermissionBannerVisibility(): void {
        // Show banner if:
        // - Any push notification is enabled
        // - AND push notification permission is not granted
        const hasPushEnabled = this.hasAnyPushNotificationsEnabled();
        const hasPermission = 'Notification' in window && Notification.permission === 'granted';

        this.showPermissionBanner.set(hasPushEnabled && !hasPermission);
        this.pushNotificationsAllowed.set(hasPermission);
    }

    async requestPushPermission(): Promise<void> {
        if (!this.pushService.isSupported()) {
            this.messageService.add({
                severity: 'warn',
                summary: 'Not Supported',
                detail: 'Push notifications are not supported in your browser',
            });
            return;
        }

        this.isRequestingPermission.set(true);

        try {
            const permission = await this.pushService.requestPermission();

            if (permission === 'granted') {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Permission Granted',
                    detail: 'Push notification permission has been granted. You will now receive push notifications.',
                });
                this.showPermissionBanner.set(false);
                this.pushNotificationsAllowed.set(true);

                // Register push subscription
                await this.registerPushSubscription();
            } else if (permission === 'denied') {
                this.messageService.add({
                    severity: 'warn',
                    summary: 'Permission Denied',
                    detail: 'Push notification permission was denied. You can enable it later in your browser settings.',
                });
            } else {
                this.messageService.add({
                    severity: 'info',
                    summary: 'Permission Not Granted',
                    detail: 'Push notification permission was not granted.',
                });
            }
        } catch (error) {
            console.error('Error requesting push permission:', error);
            this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: 'Failed to request push notification permission',
            });
        } finally {
            this.isRequestingPermission.set(false);
        }
    }

    loadPreferences(): void {
        this.isLoading.set(true);
        this.preferencesService.getDetailedPreferences().subscribe({
            next: async (data) => {
                this.preferences.set(data);
                this.isLoading.set(false);
                this.updatePermissionBannerVisibility();
                
                // Sync subscription if needed: if push is enabled and we have a browser subscription
                // but haven't registered it yet (e.g., after browser refresh or login)
                await this.syncSubscriptionIfNeeded();
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

    private async syncSubscriptionIfNeeded(): Promise<void> {
        // Only sync if:
        // 1. Push notifications are enabled in preferences
        // 2. Browser has an active subscription
        // 3. We haven't registered this subscription yet
        if (!this.hasAnyPushNotificationsEnabled()) {
            return;
        }

        const subscription = this.pushService.subscription();
        if (!subscription) {
            return;
        }

        const subscriptionData = this.pushService.getSubscriptionData();
        if (!subscriptionData) {
            return;
        }

        // If already registered with the same endpoint, skip
        if (this.isSubscriptionRegistered && this.registeredEndpoint === subscriptionData.endpoint) {
            return;
        }

        // Silently register/update the subscription on the backend
        try {
            await firstValueFrom(
                this.subscriptionsService.registerSubscription({
                    endpoint: subscriptionData.endpoint,
                    p256dh: subscriptionData.keys.p256dh,
                    auth: subscriptionData.keys.auth,
                    userAgent: navigator.userAgent,
                    expirationTimeUtc: subscription.expirationTime
                        ? new Date(subscription.expirationTime).toISOString()
                        : null,
                })
            );
            
            this.isSubscriptionRegistered = true;
            this.registeredEndpoint = subscriptionData.endpoint;
            console.log('Push subscription synced with backend');
        } catch (error) {
            console.error('Failed to sync push subscription:', error);
            // Don't show error to user - this is a background sync
        }
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

        // If disabling push, check if all available push channels are now disabled
        if (channel === 'Push' && !currentPref.isEnabled) {
            await this.handlePushChannelDisable();
        }

        // Save to server
        this.savePreferences();

        // Update permission banner visibility after preference change
        setTimeout(() => this.updatePermissionBannerVisibility(), 100);
    }

    private async handlePushChannelDisable(): Promise<void> {
        // Check if all available push channels are disabled
        const prefs = this.preferences();
        if (!prefs) return;

        const allPushDisabled = prefs.notificationTypes.every((type) => {
            const pushPref = type.channelPreferences['Push'];
            return !pushPref?.isAvailable || !pushPref?.isEnabled;
        });

        if (allPushDisabled) {
            // All push channels are disabled, unsubscribe from push
            const subscriptionData = this.pushService.getSubscriptionData();
            if (subscriptionData) {
                try {
                    await firstValueFrom(
                        this.subscriptionsService.unsubscribe(subscriptionData.endpoint)
                    );
                    
                    // Clear registration tracking
                    this.isSubscriptionRegistered = false;
                    this.registeredEndpoint = null;
                    
                    this.messageService.add({
                        severity: 'info',
                        summary: 'Unsubscribed',
                        detail: 'You have been unsubscribed from push notifications',
                    });
                } catch (error) {
                    console.error('Failed to unsubscribe from push notifications:', error);
                    this.messageService.add({
                        severity: 'warn',
                        summary: 'Unsubscribe Failed',
                        detail: 'Could not unsubscribe from push notifications. Please try again.',
                    });
                }
            }
        }
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

            // Check if this exact subscription is already registered
            if (this.isSubscriptionRegistered && this.registeredEndpoint === subscriptionData.endpoint) {
                console.log('Subscription already registered, skipping duplicate registration');
                return true;
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

            // Mark as registered
            this.isSubscriptionRegistered = true;
            this.registeredEndpoint = subscriptionData.endpoint;

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
                this.updatePermissionBannerVisibility();
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
