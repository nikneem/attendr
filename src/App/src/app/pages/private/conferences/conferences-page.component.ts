import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { ConferencesService } from '../../../shared/services/conferences.service';
import { ConferenceListItemDto } from '../../../shared/models/conference-list-item-dto';
import { CreateConferenceComponent } from '../../../shared/components/create-conference/create-conference.component';

@Component({
    selector: 'attn-conferences-page',
    imports: [CommonModule, ButtonModule, CardModule, DialogModule, ProgressSpinnerModule, TooltipModule, CreateConferenceComponent],
    templateUrl: './conferences-page.component.html',
    styleUrl: './conferences-page.component.scss',
})
export class ConferencesPageComponent implements OnInit {
    private readonly conferencesService = inject(ConferencesService);
    private readonly router = inject(Router);
    private readonly cdr = inject(ChangeDetectorRef);

    conferences: ConferenceListItemDto[] = [];
    loading = true;
    error: string | null = null;
    showCreateDialog = false;

    ngOnInit(): void {
        // Defer loading to avoid ExpressionChangedAfterItHasBeenCheckedError
        setTimeout(() => this.loadConferences(), 0);
    }

    loadConferences(): void {
        this.loading = true;
        this.error = null;

        this.conferencesService
            .listConferences(undefined, 50, 1)
            .subscribe({
                next: (result) => {
                    this.conferences = result.conferences;
                    this.loading = false;
                    this.cdr.markForCheck();
                },
                error: (err) => {
                    this.error = err.message || 'Failed to load conferences';
                    this.loading = false;
                    this.cdr.markForCheck();
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

    viewConference(conferenceId: string): void {
        this.router.navigate(['/app/conferences', conferenceId]);
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
