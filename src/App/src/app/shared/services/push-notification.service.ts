import { Injectable, signal } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class PushNotificationService {
    readonly isSupported = signal(this.checkPushSupport());
    readonly isSubscribed = signal(false);
    readonly subscription = signal<PushSubscription | null>(null);

    constructor() {
        this.checkSubscriptionStatus();
    }

    /**
     * Check if push notifications are supported in this browser
     */
    private checkPushSupport(): boolean {
        return 'serviceWorker' in navigator && 'PushManager' in window;
    }

    /**
     * Request permission for push notifications
     */
    async requestPermission(): Promise<NotificationPermission> {
        if (!this.isSupported()) {
            throw new Error('Push notifications are not supported in this browser');
        }

        return Notification.requestPermission();
    }

    /**
     * Subscribe to push notifications
     */
    async subscribe(vapidPublicKey: string): Promise<PushSubscription> {
        if (!this.isSupported()) {
            throw new Error('Push notifications are not supported');
        }

        const permission = await this.requestPermission();
        if (permission !== 'granted') {
            throw new Error('Notification permission not granted');
        }

        const registration = await navigator.serviceWorker.ready;
        const subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: this.urlBase64ToUint8Array(vapidPublicKey) as any
        } as any);

        this.subscription.set(subscription);
        this.isSubscribed.set(true);

        return subscription;
    }

    /**
     * Unsubscribe from push notifications
     */
    async unsubscribe(): Promise<boolean> {
        const sub = this.subscription();
        if (!sub) {
            return false;
        }

        const unsubscribed = await sub.unsubscribe();
        if (unsubscribed) {
            this.subscription.set(null);
            this.isSubscribed.set(false);
        }

        return unsubscribed;
    }

    /**
     * Get current subscription status
     */
    private async checkSubscriptionStatus(): Promise<void> {
        if (!this.isSupported()) {
            return;
        }

        try {
            const registration = await navigator.serviceWorker.ready;
            const subscription = await registration.pushManager.getSubscription();

            if (subscription) {
                this.subscription.set(subscription);
                this.isSubscribed.set(true);
            }
        } catch (error) {
            console.error('Error checking push subscription status:', error);
        }
    }

    /**
     * Get the subscription endpoint and keys for server storage
     */
    getSubscriptionData(): { endpoint: string; keys: { p256dh: string; auth: string } } | null {
        const sub = this.subscription();
        if (!sub) {
            return null;
        }

        return {
            endpoint: sub.endpoint,
            keys: {
                p256dh: this.arrayBufferToBase64(sub.getKey('p256dh')),
                auth: this.arrayBufferToBase64(sub.getKey('auth'))
            }
        };
    }

    /**
     * Convert VAPID public key from base64 to Uint8Array
     */
    private urlBase64ToUint8Array(base64String: string): Uint8Array {
        const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        const base64 = (base64String + padding).replace(/\-/g, '+').replace(/_/g, '/');

        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }

        return outputArray;
    }

    /**
     * Convert ArrayBuffer to Base64 string
     */
    private arrayBufferToBase64(buffer: ArrayBuffer | null): string {
        if (!buffer) return '';
        const bytes = new Uint8Array(buffer);
        let binary = '';
        for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary);
    }
}
