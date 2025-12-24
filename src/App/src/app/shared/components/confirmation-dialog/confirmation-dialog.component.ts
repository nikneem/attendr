import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';

@Component({
    selector: 'attn-confirmation-dialog',
    standalone: true,
    imports: [CommonModule, DialogModule, ButtonModule],
    templateUrl: './confirmation-dialog.component.html',
    styleUrl: './confirmation-dialog.component.scss',
})
export class ConfirmationDialogComponent {
    @Input() title = 'Confirm';
    @Input() message = 'Are you sure?';
    @Input() visible = false;

    @Output() visibleChange = new EventEmitter<boolean>();
    @Output() confirm = new EventEmitter<void>();
    @Output() cancel = new EventEmitter<void>();

    onHide(): void {
        this.visibleChange.emit(false);
    }

    onConfirm(): void {
        this.confirm.emit();
        this.onHide();
    }

    onCancel(): void {
        this.cancel.emit();
        this.onHide();
    }
}
