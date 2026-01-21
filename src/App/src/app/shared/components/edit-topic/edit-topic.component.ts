import { Component, input, output, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { CommonModule } from '@angular/common';
import { TopicDto } from '../../services/topics.service';

@Component({
    selector: 'attn-edit-topic',
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, InputTextModule, ToggleSwitchModule],
    templateUrl: './edit-topic.component.html',
    styleUrl: './edit-topic.component.scss',
})
export class EditTopicComponent {
    topic = input.required<TopicDto>();

    // Local form state
    key = '';
    name = '';
    isVisible = false;

    // Save enabled only when changes are made
    isDirty = false;
    isSaving = false;

    save = output<{ key: string; name: string; isVisible: boolean }>();
    delete = output<void>();
    cancel = output<void>();

    constructor() {
        effect(() => {
            const t = this.topic();
            if (t) {
                this.key = t.key;
                this.name = t.name;
                this.isVisible = t.isVisible;
                this.isDirty = false;
                this.isSaving = false;
            }
        });
    }

    onKeyChange(value: string): void {
        this.key = value;
        this.markDirtyIfChanged();
    }

    onNameChange(value: string): void {
        this.name = value;
        this.markDirtyIfChanged();
    }

    onVisibleChange(value: boolean): void {
        this.isVisible = value;
        this.markDirtyIfChanged();
    }

    private markDirtyIfChanged(): void {
        const t = this.topic();
        this.isDirty = !!t && (this.key !== t.key || this.name !== t.name || this.isVisible !== t.isVisible);
    }

    onSave(): void {
        if (!this.isDirty || this.isSaving) {
            return;
        }
        this.isSaving = true;
        this.save.emit({ key: this.key.trim(), name: this.name.trim(), isVisible: this.isVisible });
    }

    onDelete(): void {
        this.delete.emit();
    }

    onCancel(): void {
        this.cancel.emit();
    }
}
