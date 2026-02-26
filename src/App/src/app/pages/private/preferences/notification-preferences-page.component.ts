import { Component, effect, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule, KeyValue } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { NotificationPreferencesService } from '@services/notification-preferences.service';
import { NotificationSubscriptionsService } from '@services/notification-subscriptions.service';
import { PushNotificationService } from '@services/push-notification.service';
import { NotificationPreferencesDetailDto, NotificationTypePreferenceDto, ChannelPreferenceDto } from '@models/notification-preferences-detail-dto';
import { UpdateDetailedPreferencesRequest } from '@models/update-notification-preferences-request';
import { environment } from '../../../../environments/environment';

export type StepStatus = 'complete' | 'incomplete' | 'blocked';

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
        TagModule,
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
    isRegistering = signal(false);
    isUnsubscribing = signal(false);

    // PWA Detection signals
    isMobileDevice = signal(false);
    isPwaInstalled = signal(false);
    deferredPromptAvailable = signal(false);
    notificationPermission = signal<NotificationPermission>(
        'Notification' in window ? Notification.permission : 'default'
    );

    // Push setup state
    isSubscriptionRegistered = signal(false);
    private registeredEndpoint: string | null = null;
    registrationError = signal<string | null>(null);
    testSentCount = signal<number | null>(null);
    testSendError = signal<string | null>(null);

    // Step status computed signals
    showInstallStep = computed(() =>
        this.isMobileDevice() && !this.isPwaInstalled() && this.deferredPromptAvailable()
    );

    installStepStatus = computed<StepStatus>(() => {
        if (!this.showInstallStep()) return 'complete';
        return this.isPwaInstalled() ? 'complete' : 'incomplete';
    });

    permissionStepStatus = computed<StepStatus>(() => {
        if (this.installStepStatus() === 'incomplete') return 'blocked';
        const perm = this.notificationPermission();
        if (perm === 'granted') return 'complete';
        if (perm === 'denied') return 'blocked';
        return 'incomplete';
    });

    registerStepStatus = computed<StepStatus>(() => {
        if (this.permissionStepStatus() !== 'complete') return 'blocked';
        return this.isSubscriptionRegistered() ? 'complete' : 'incomplete';
    });

    testStepStatus = computed<StepStatus>(() => {
        if (this.registerStepStatus() !== 'complete') return 'blocked';
        return this.testSentCount() !== null ? 'complete' : 'incomplete';
    });

    hasPushEnabledButNotRegistered = computed(() =>
        this.hasAnyPushNotificationsEnabled() && !this.isSubscriptionRegistered()
    );

    // Keep for backward compat (used in updatePermissionBannerVisibility)
    pushNotificationsAllowed = computed(() => this.notificationPermission() === 'granted');
    isRequestingPermission = signal(false);
    isInstallingApp = signal(false);

    constructor() {
        // When the service worker subscription signal resolves (async after SW ready),
        // automatically perform the idempotent registration if permission is already granted.
        effect(() => {
            const sub = this.pushService.subscription();
            const perm = this.notificationPermission();
            if (sub && perm === 'granted' && !this.isSubscriptionRegistered()) {
                this.syncSubscriptionSilently();
            }
        });
    }

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

        // Detect current permission state
        if ('Notification' in window) {
            this.notificationPermission.set(Notification.permission);
        }
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
        this.deferredPromptAvailable.set(true);
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
                this.isPwaInstalled.set(true);
                this.deferredPromptAvailable.set(false);
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
        this.deferredPromptAvailable.set(false);
        this.deferredPrompt = null;
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
        if ('Notification' in window) {
            this.notificationPermission.set(Notification.permission);
        }
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
                this.notificationPermission.set('granted');

                // Register push subscription
                await this.registerPushSubscription();
            } else if (permission === 'denied') {
                this.notificationPermission.set('denied');
                this.messageService.add({
                    severity: 'warn',
                    summary: 'Permission Denied',
                    detail: 'Push notification permission was denied. You can enable it later in your browser settings.',
                });
            } else {
                this.notificationPermission.set(permission);
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
        await this.syncSubscriptionSilently();
    }

    /**
     * Idempotent background registration: post the current browser subscription to the backend
     * so the server always knows about this device. Safe to call on every page load.
     */
    private async syncSubscriptionSilently(): Promise<void> {
        const subscription = this.pushService.subscription();
        if (!subscription) {
            return;
        }

        const subscriptionData = this.pushService.getSubscriptionData();
        if (!subscriptionData) {
            return;
        }

        // If already registered with the same endpoint in this session, skip
        if (this.isSubscriptionRegistered() && this.registeredEndpoint === subscriptionData.endpoint) {
            return;
        }

        // Silently register/update the subscription on the backend (idempotent)
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

            this.isSubscriptionRegistered.set(true);
            this.registeredEndpoint = subscriptionData.endpoint;
            this.registrationError.set(null);
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
                    this.isSubscriptionRegistered.set(false);
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
        const permission = this.notificationPermission();

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
                this.notificationPermission.set(newPermission);
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
            if (this.isSubscriptionRegistered() && this.registeredEndpoint === subscriptionData.endpoint) {
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
            this.isSubscriptionRegistered.set(true);
            this.registeredEndpoint = subscriptionData.endpoint;
            this.registrationError.set(null);

            return true;
        } catch (error) {
            console.error('Failed to register push subscription:', error);
            this.registrationError.set('Could not register your push subscription. Please try again.');
            this.messageService.add({
                severity: 'error',
                summary: 'Push Registration Failed',
                detail: 'Could not register your push subscription. Please try again.',
            });
            return false;
        }
    }

    /** Explicit "Register this device" action triggered from the setup card. */
    async registerDevice(): Promise<void> {
        this.isRegistering.set(true);
        this.registrationError.set(null);
        try {
            await this.registerPushSubscription();
        } finally {
            this.isRegistering.set(false);
        }
    }

    /** Unsubscribe this device: calls backend DELETE then browser unsubscribe. */
    async unsubscribeDevice(): Promise<void> {
        this.isUnsubscribing.set(true);
        try {
            const subscriptionData = this.pushService.getSubscriptionData();
            if (subscriptionData) {
                await firstValueFrom(
                    this.subscriptionsService.unsubscribe(subscriptionData.endpoint)
                );
            }
            await this.pushService.unsubscribe();
            this.isSubscriptionRegistered.set(false);
            this.registeredEndpoint = null;
            this.testSentCount.set(null);
            this.messageService.add({
                severity: 'info',
                summary: 'Unsubscribed',
                detail: 'This device has been unsubscribed from push notifications.',
            });
        } catch (error) {
            console.error('Failed to unsubscribe:', error);
            this.messageService.add({
                severity: 'error',
                summary: 'Unsubscribe Failed',
                detail: 'Could not unsubscribe this device. Please try again.',
            });
        } finally {
            this.isUnsubscribing.set(false);
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
        this.testSentCount.set(null);
        this.testSendError.set(null);
        this.subscriptionsService.sendTestNotification().subscribe({
            next: (response) => {
                this.testSentCount.set(response.sentCount);
                this.messageService.add({
                    severity: 'success',
                    summary: 'Test Sent',
                    detail: response.message,
                });
                this.isSendingTest.set(false);
            },
            error: (error) => {
                console.error('Failed to send test notification:', error);
                this.testSendError.set('Failed to send test notification. Please try again.');
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
