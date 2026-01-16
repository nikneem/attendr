import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, KeyValue } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { NotificationPreferencesService } from '@services/notification-preferences.service';
import { NotificationPreferencesDetailDto, NotificationTypePreferenceDto, ChannelPreferenceDto } from '@models/notification-preferences-detail-dto';
import { UpdateDetailedPreferencesRequest } from '@models/update-notification-preferences-request';

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
    private readonly messageService = inject(MessageService);

    readonly channelKeys = ['InApp', 'Email', 'Push'];

    preferences = signal<NotificationPreferencesDetailDto | null>(null);
    isLoading = signal(true);
    isSaving = signal(false);

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

    onChannelToggle(notificationType: NotificationTypePreferenceDto, channel: string): void {
        const currentPref = notificationType.channelPreferences[channel];
        if (!currentPref) return;

        // Only allow toggling if channel is available
        if (!currentPref.isAvailable) return;

        // Toggle the value
        currentPref.isEnabled = !currentPref.isEnabled;

        // Save to server
        this.savePreferences();
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
}
