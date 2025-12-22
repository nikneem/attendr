import { Component, inject, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { JoinedGroupsService } from '../../services/joined-groups.service';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'attn-create-group',
    imports: [FormsModule, ButtonModule, InputTextModule],
    templateUrl: './create-group.component.html',
    styleUrl: './create-group.component.scss',
})
export class CreateGroupComponent {
    private readonly joinedGroupsService = inject(JoinedGroupsService);
    private readonly messageService = inject(MessageService);

    groupName = '';
    isCreating = false;
    validationError = '';

    groupCreated = output<{ id: string; name: string; memberCount: number }>();

    validateGroupName(): boolean {
        this.validationError = '';

        if (!this.groupName.trim()) {
            this.validationError = 'Group name is required';
            return false;
        }

        if (this.groupName.trim().length < 3) {
            this.validationError = 'Group name must be at least 3 characters long';
            return false;
        }

        if (this.groupName.trim().length > 100) {
            this.validationError = 'Group name must not exceed 100 characters';
            return false;
        }

        const alphanumericPattern = /^[a-zA-Z0-9\s]+$/;
        if (!alphanumericPattern.test(this.groupName)) {
            this.validationError = 'Group name can only contain alphanumeric characters and spaces';
            return false;
        }

        return true;
    }

    onSubmit() {
        if (!this.validateGroupName()) {
            return;
        }

        this.isCreating = true;
        this.joinedGroupsService.createGroup({ name: this.groupName.trim() }).subscribe({
            next: (result) => {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: `Group "${result.name}" created successfully`,
                });

                // Emit the new group to parent component
                this.groupCreated.emit({
                    id: result.id,
                    name: result.name,
                    memberCount: result.members.length,
                });

                // Reset form
                this.groupName = '';
                this.isCreating = false;
            },
            error: (error) => {
                this.isCreating = false;
                const errorMessage = error.error?.error || 'Failed to create group';
                this.validationError = errorMessage;
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: errorMessage,
                });
            },
        });
    }
}
