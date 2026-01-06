import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { PresenceService } from '@services/presence.service';
import { ConferencePresenceDto } from '@models/conference-presence-dto';
import { MessageService } from 'primeng/api';

interface ConferenceRow {
    conferenceId: string;
    conferenceName: string;
    location: string;
    startDate: string;
    endDate: string;
    isAttending: boolean;
    ratingRequired: boolean;
}

@Component({
    selector: 'attn-my-conferences',
    standalone: true,
    imports: [CommonModule, CardModule, ButtonModule],
    templateUrl: './my-conferences.component.html',
    styleUrl: './my-conferences.component.scss',
})
export class MyConferencesComponent implements OnInit {
    private readonly presenceService = inject(PresenceService);
    private readonly messageService = inject(MessageService);

    conferences = signal<ConferenceRow[]>([]);
    loading = signal(true);
    error = signal<string | null>(null);
    updatingConference = signal<string | null>(null);

    ngOnInit(): void {
        this.loadConferences();
    }

    loadConferences(): void {
        this.loading.set(true);
        this.error.set(null);

        this.presenceService.getMyConferences().subscribe({
            next: (presences: ConferencePresenceDto[]) => {
                // Filter to current/future conferences
                const now = new Date();
                this.conferences.set(presences
                    .filter(p => new Date(p.endDate) >= now)
                    .map(p => ({
                        conferenceId: p.conferenceId,
                        conferenceName: p.conferenceName,
                        location: p.location,
                        startDate: p.startDate,
                        endDate: p.endDate,
                        isAttending: p.isAttending,
                        ratingRequired: false, // Will be computed from presentations via API if needed
                    }))
                    .sort((a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime()));

                this.loading.set(false);
            },
            error: (err) => {
                console.error('Error loading conferences:', err);
                this.error.set('Failed to load your conferences. Please try again.');
                this.loading.set(false);
            },
        });
    }

    updateAttendance(conferenceId: string, isAttending: boolean): void {
        this.updatingConference.set(conferenceId);

        this.presenceService.updateAttendance(conferenceId, isAttending).subscribe({
            next: () => {
                // Update the local state
                const updatedConferences = this.conferences().map(c =>
                    c.conferenceId === conferenceId ? { ...c, isAttending } : c
                );
                this.conferences.set(updatedConferences);
                this.updatingConference.set(null);

                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: isAttending ? 'You are now attending this conference' : 'You are no longer attending this conference'
                });
            },
            error: (err) => {
                console.error('Error updating attendance:', err);
                this.updatingConference.set(null);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to update attendance. Please try again.'
                });
            }
        });
    }

    formatDateRange(startDate: string, endDate: string): string {
        const start = new Date(startDate);
        const end = new Date(endDate);
        const options: Intl.DateTimeFormatOptions = { month: 'short', day: 'numeric' };

        if (start.getFullYear() === end.getFullYear() && start.getMonth() === end.getMonth()) {
            return `${start.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${end.toLocaleDateString('en-US', { day: 'numeric', year: 'numeric' })}`;
        }
        return `${start.toLocaleDateString('en-US', options)} - ${end.toLocaleDateString('en-US', { ...options, year: 'numeric' })}`;
    }
}
