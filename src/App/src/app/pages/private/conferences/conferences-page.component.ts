import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { ConferencesService } from '../../../shared/services/conferences.service';
import { ConferenceListItemDto } from '../../../shared/models/conference-list-item-dto';
import { CreateConferenceComponent } from '../../../shared/components/create-conference/create-conference.component';

@Component({
    selector: 'attn-conferences-page',
    imports: [CommonModule, ButtonModule, CardModule, DialogModule, CreateConferenceComponent],
    templateUrl: './conferences-page.component.html',
    styleUrl: './conferences-page.component.scss',
})
export class ConferencesPageComponent implements OnInit {
    private readonly conferencesService = inject(ConferencesService);

    conferences: ConferenceListItemDto[] = [];
    loading = false;
    error: string | null = null;
    showCreateDialog = false;

    ngOnInit(): void {
        this.loadConferences();
    }

    loadConferences(): void {
        this.loading = true;
        this.error = null;

        this.conferencesService.listConferences(undefined, 50, 1).subscribe({
            next: (result) => {
                this.conferences = result.conferences;
                this.loading = false;
            },
            error: (err) => {
                this.error = err.message || 'Failed to load conferences';
                this.loading = false;
            },
        });
    }

    onAddConference(): void {
        this.showCreateDialog = true;
    }

    onConferenceCreated(conference: { id: string; title: string }): void {
        this.showCreateDialog = false;
        this.loadConferences(); // Reload the list to show the new conference
    }

    formatDate(dateString: string): string {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
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
