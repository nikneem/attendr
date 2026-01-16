export interface RegisterPushSubscriptionRequest {
    endpoint: string;
    p256dh: string;
    auth: string;
    userAgent: string;
    expirationTimeUtc: string | null;
}
