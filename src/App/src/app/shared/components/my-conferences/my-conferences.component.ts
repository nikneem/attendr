import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { PresenceService } from '@services/presence.service';
import { ConferencePresenceDto } from '@models/conference-presence-dto';

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
    imports: [CommonModule, CardModule, TableModule, TagModule],
    templateUrl: './my-conferences.component.html',
    styleUrl: './my-conferences.component.scss',
})
export class MyConferencesComponent implements OnInit {
    private readonly presenceService = inject(PresenceService);

    conferences = signal<ConferenceRow[]>([]);
    loading = signal(true);
    error = signal<string | null>(null);

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

    getAttendanceText(isAttending: boolean): string {
        return isAttending ? 'Yes' : 'No';
    }

    getAttendanceSeverity(isAttending: boolean): 'success' | 'warn' {
        return isAttending ? 'success' : 'warn';
    }

    getRatingRequiredText(ratingRequired: boolean): string {
        return ratingRequired ? 'Yes' : 'No';
    }

    getRatingRequiredSeverity(ratingRequired: boolean): 'danger' | 'info' {
        return ratingRequired ? 'danger' : 'info';
    }
}
