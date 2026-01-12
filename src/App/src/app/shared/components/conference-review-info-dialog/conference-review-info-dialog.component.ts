import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';

@Component({
    selector: 'attn-conference-review-info-dialog',
    standalone: true,
    imports: [CommonModule, ButtonModule],
    templateUrl: './conference-review-info-dialog.component.html',
    styleUrl: './conference-review-info-dialog.component.scss',
})
export class ConferenceReviewInfoDialogComponent {
    conferenceTitle = input.required<string>();
    conferenceId = input.required<string>();

    dialogClosed = output<void>();

    onClose(): void {
        this.dialogClosed.emit();
    }
}
