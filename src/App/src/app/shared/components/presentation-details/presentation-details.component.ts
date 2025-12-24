import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PresentationDto } from '@models/presentation-dto';

@Component({
    selector: 'attn-presentation-details',
    imports: [CommonModule],
    templateUrl: './presentation-details.component.html',
    styleUrl: './presentation-details.component.scss',
})
export class PresentationDetailsComponent {
    presentation = input.required<PresentationDto>();
    isFavorite = input<boolean>(false);

    favoriteToggled = output<boolean>();

    getSpeakerNames(): string {
        const presentation = this.presentation();
        return presentation.speakers.map(s => s.name).join(', ');
    }

    getSpeakerImages() {
        const presentation = this.presentation();
        return presentation.speakers.filter(s => s.profilePictureUrl);
    }

    formatDateTime(dateString: string): string {
        const date = new Date(dateString);
        return date.toLocaleString('en-US', {
            month: 'long',
            day: 'numeric',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        });
    }

    toggleFavorite(): void {
        this.favoriteToggled.emit(!this.isFavorite());
    }
}
