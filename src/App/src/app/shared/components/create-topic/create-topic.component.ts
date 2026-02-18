import { Component, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'attn-create-topic',
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, InputTextModule],
    templateUrl: './create-topic.component.html',
    styleUrl: './create-topic.component.scss',
})
export class CreateTopicComponent {
    key = signal<string>('');
    name = signal<string>('');
    isSaving = signal<boolean>(false);

    save = output<{ key: string; name: string }>();
    cancel = output<void>();

    get isValid(): boolean {
        return this.key().trim().length > 0 && this.name().trim().length > 0;
    }

    onKeyChange(value: string): void {
        // Auto-normalize: lowercase, replace spaces with hyphens, strip non-alphanumeric except hyphens
        const normalized = value.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '');
        this.key.set(normalized);
    }

    onNameChange(value: string): void {
        this.name.set(value);
    }

    onSave(): void {
        if (!this.isValid || this.isSaving()) {
            return;
        }
        this.save.emit({ key: this.key().trim(), name: this.name().trim() });
    }

    onCancel(): void {
        this.cancel.emit();
    }

    reset(): void {
        this.key.set('');
        this.name.set('');
        this.isSaving.set(false);
    }
}
