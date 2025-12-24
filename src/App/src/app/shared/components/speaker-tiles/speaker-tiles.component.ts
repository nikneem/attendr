import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SpeakerDto } from '@models/speaker-dto';

@Component({
    selector: 'attn-speaker-tiles',
    imports: [CommonModule],
    templateUrl: './speaker-tiles.component.html',
    styleUrl: './speaker-tiles.component.scss',
})
export class SpeakerTilesComponent {
    speakers = input.required<SpeakerDto[]>();

    sortedSpeakers = computed(() => {
        const speakers = this.speakers();
        return [...speakers].sort((a, b) => a.name.localeCompare(b.name));
    });

    getInitials(name: string): string {
        return name
            .split(' ')
            .map((n) => n[0])
            .join('')
            .toUpperCase()
            .substring(0, 2);
    }
}
