import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ConferencesService } from '../../../shared/services/conferences.service';
import { ConferenceDetailsDto } from '../../../shared/models/conference-details-dto';

@Component({
    selector: 'attn-conference-details-page',
    imports: [CommonModule, CardModule, TagModule, ProgressSpinnerModule],
    templateUrl: './conference-details-page.component.html',
    styleUrl: './conference-details-page.component.scss',
})
export class ConferenceDetailsPageComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly conferencesService = inject(ConferencesService);

    conference: ConferenceDetailsDto | null = null;
    loading = true;
    error: string | null = null;

    ngOnInit(): void {
        const conferenceId = this.route.snapshot.paramMap.get('id');
        if (conferenceId) {
            this.loadConference(conferenceId);
        } else {
            this.error = 'Conference ID not found';
            this.loading = false;
        }
    }

    loadConference(id: string): void {
        this.loading = true;
        this.error = null;

        this.conferencesService.getConference(id).subscribe({
            next: (conference) => {
                this.conference = conference;
                this.loading = false;
            },
            error: (err) => {
                this.error = err.status === 404 ? 'Conference not found' : 'Failed to load conference';
                this.loading = false;
            },
        });
    }

    formatDate(dateString: string): string {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });
    }

    formatDateRange(startDate: string, endDate: string): string {
        const start = new Date(startDate);
        const end = new Date(endDate);

        if (start.getTime() === end.getTime()) {
            return this.formatDate(startDate);
        }

        return `${this.formatDate(startDate)} - ${this.formatDate(endDate)}`;
    }
}
