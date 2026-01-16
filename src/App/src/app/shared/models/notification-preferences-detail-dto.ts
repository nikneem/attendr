export interface ChannelPreferenceDto {
    channelName: string;
    isAvailable: boolean;
    isEnabled: boolean;
    isDefaultEnabled: boolean;
}

export interface NotificationTypePreferenceDto {
    typeKey: string;
    displayName: string;
    description: string;
    channelPreferences: { [key: string]: ChannelPreferenceDto };
}

export interface NotificationPreferencesDetailDto {
    profileId: string;
    updatedAt: string | null;
    doNotDisturbUntil: string | null;
    notificationTypes: NotificationTypePreferenceDto[];
}
