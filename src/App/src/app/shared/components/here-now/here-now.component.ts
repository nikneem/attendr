import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { PresenceService } from '@services/presence.service';
import { CurrentConferenceDto } from '@models/current-conference-dto';

@Component({
    selector: 'attn-here-now',
    standalone: true,
    imports: [CommonModule, CardModule],
    templateUrl: './here-now.component.html',
    styleUrl: './here-now.component.scss',
})
export class HereNowComponent implements OnInit {
    private readonly presenceService = inject(PresenceService);

    conferences = signal<CurrentConferenceDto[]>([]);
    loading = signal<boolean>(true);
    hasConferences = signal<boolean>(false);

    ngOnInit() {
        this.loadCurrentConferences();
    }

    private loadCurrentConferences() {
        this.loading.set(true);
        this.presenceService.getCurrentConferences().subscribe({
            next: conferences => {
                this.conferences.set(conferences);
                this.hasConferences.set(conferences.length > 0);
                this.loading.set(false);
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
}
