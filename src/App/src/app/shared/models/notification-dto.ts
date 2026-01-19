export interface NotificationDto {
    id: string;
    profileId: string;
    typeKey: string;
    severity: string;
    title: string;
    message: string;
    url?: string;
    actorId?: string;
    entityRefs?: { [key: string]: string };
    count: number;
    createdAt: Date;
    lastOccurredAt?: Date;
    readAt?: Date;
    isRead: boolean;
    channelDeliveries?: { [key: string]: ChannelDeliveryDto };
}

export interface ChannelDeliveryDto {
    enabled: boolean;
    status: string;
    deliveredAt?: Date;
    errorMessage?: string;
}
