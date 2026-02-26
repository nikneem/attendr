import { Component, OnInit, inject, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { ToastModule } from 'primeng/toast';
import { MenuItem, MessageService } from 'primeng/api';
import { PresenceService } from '@services/presence.service';
import { CurrentConferenceDto } from '@models/current-conference-dto';
import { ConferenceScheduleNowDto, ScheduledPresentationDto } from '@models/conference-schedule-now-dto';
import { forkJoin } from 'rxjs';

interface ConferenceWithSchedule extends CurrentConferenceDto {
    schedule?: ConferenceScheduleNowDto;
}

import { TranslateModule } from '@ngx-translate/core';
@Component({
    selector: 'attn-here-now',
    standalone: true,
    imports: [CommonModule, CardModule, ContextMenuModule, ToastModule,
        TranslateModule
    ],
    providers: [MessageService],
    templateUrl: './here-now.component.html',
    styleUrl: './here-now.component.scss',
})
export class HereNowComponent implements OnInit {
    private readonly presenceService = inject(PresenceService);
    private readonly messageService = inject(MessageService);

    @ViewChild('cm') contextMenu!: ContextMenu;

    conferences = signal<ConferenceWithSchedule[]>([]);
    loading = signal<boolean>(true);
    hasConferences = signal<boolean>(false);
    contextMenuItems = signal<MenuItem[]>([]);
    selectedPresentation = signal<{ conferenceId: string; presentation: ScheduledPresentationDto } | null>(null);

    ngOnInit() {
        this.loadCurrentConferences();
    }

    private loadCurrentConferences() {
        this.loading.set(true);
        this.presenceService.getCurrentConferences().subscribe({
            next: conferences => {
                if (conferences.length > 0) {
                    // Load schedule for each conference
                    const scheduleRequests = conferences.map(conf =>
                        this.presenceService.getConferenceScheduleNow(conf.conferenceId)
                    );

                    forkJoin(scheduleRequests).subscribe({
                        next: schedules => {
                            const conferencesWithSchedule = conferences.map((conf, index) => ({
                                ...conf,
                                schedule: schedules[index],
                            }));
                            this.conferences.set(conferencesWithSchedule);
                            this.hasConferences.set(true);
                            this.loading.set(false);
                        },
                        error: error => {
                            console.error('Error loading schedules:', error);
                            this.conferences.set(conferences);
                            this.hasConferences.set(true);
                            this.loading.set(false);
                        },
                    });
                } else {
                    this.conferences.set([]);
                    this.hasConferences.set(false);
                    this.loading.set(false);
                }
            },
            error: error => {
                console.error('Error loading current conferences:', error);
                this.loading.set(false);
                this.hasConferences.set(false);
            },
        });
    }

    getConferenceImage(conference: CurrentConferenceDto): string {
        return conference.imageUrl || 'assets/placeholder-conference.jpg';
    }

    getSortedPresentations(presentations: ScheduledPresentationDto[]): ScheduledPresentationDto[] {
        if (!presentations || presentations.length === 0) return [];
        return [...presentations].sort((a, b) => {
            if (a.isPreferred && !b.isPreferred) return -1;
            if (!a.isPreferred && b.isPreferred) return 1;
            return 0;
        });
    }

    getSessionOpacity(presentation: ScheduledPresentationDto, presentations: ScheduledPresentationDto[]): number {
        if (presentations.length === 1) return 1;
        return presentation.isPreferred ? 1 : 0.6;
    }

    onSessionClick(event: MouseEvent, conferenceId: string, presentation: ScheduledPresentationDto) {
        event.preventDefault();
        this.selectedPresentation.set({ conferenceId, presentation });

        this.contextMenuItems.set([
            {
                label: 'Check In',
                icon: 'pi pi-check-circle',
                command: () => this.checkInToSession(),
            },
            {
                label: presentation.isPreferred ? 'Unmark as Preferred' : 'Mark as Preferred',
                icon: presentation.isPreferred ? 'pi pi-star-fill' : 'pi pi-star',
                command: () => this.togglePreferredSession(),
            },
        ]);

        if (this.contextMenu) {
            this.contextMenu.show(event);
        }
    }

    private checkInToSession() {
        const selected = this.selectedPresentation();
        if (!selected) return;

        this.presenceService
            .checkInToPresentation(selected.conferenceId, selected.presentation.presentationId)
            .subscribe({
                next: () => {
                    this.messageService.add({
                        severity: 'success',
                        summary: 'Checked In',
                        detail: `Successfully checked in to "${selected.presentation.title}"`,
                    });
                },
                error: error => {
                    console.error('Error checking in:', error);
                    this.messageService.add({
                        severity: 'error',
                        summary: 'Check-in Failed',
                        detail: 'Failed to check in to the session',
                    });
                },
            });
    }

    private togglePreferredSession() {
        const selected = this.selectedPresentation();
        if (!selected) return;

        this.presenceService
            .setPreferredPresentation(selected.conferenceId, selected.presentation.presentationId)
            .subscribe({
                next: () => {
                    this.messageService.add({
                        severity: 'success',
                        summary: 'Preference Updated',
                        detail: 'Presentation preference has been updated',
                    });
                    // Reload to refresh the preference status
                    this.loadCurrentConferences();
                },
                error: error => {
                    console.error('Error updating preference:', error);
                    this.messageService.add({
                        severity: 'error',
                        summary: 'Update Failed',
                        detail: 'Failed to update presentation preference',
                    });
                },
            });
    }

    formatTime(dateTime: string): string {
        return new Date(dateTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }
}
