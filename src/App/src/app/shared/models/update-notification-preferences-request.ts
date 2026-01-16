export interface UpdateNotificationTypePreferenceRequest {
    typeKey: string;
    channelPreferences: { [key: string]: boolean };
}

export interface UpdateDetailedPreferencesRequest {
    notificationTypes: UpdateNotificationTypePreferenceRequest[];
}
