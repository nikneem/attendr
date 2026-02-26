import { Component, inject, input, output, OnInit, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';
import { JoinedGroupsService } from '../../services/joined-groups.service';
import { GroupDetailsDto } from '../../models/group-details-dto';

import { TranslateModule } from '@ngx-translate/core';
@Component({
    selector: 'attn-edit-group',
    imports: [FormsModule, ButtonModule, InputTextModule, CheckboxModule,
        TranslateModule
    ],
    templateUrl: './edit-group.component.html',
    styleUrl: './edit-group.component.scss',
})
export class EditGroupComponent implements OnInit {
    private readonly groupsService = inject(JoinedGroupsService);
    private readonly messageService = inject(MessageService);

    group = input.required<GroupDetailsDto>();
    groupUpdated = output<void>();

    groupName = signal('');
    isPublic = signal(false);
    isSearchable = signal(false);
    isUpdating = signal(false);
    validationError = signal('');

    // Computed signal to disable public checkbox when not searchable
    isPublicDisabled = computed(() => !this.isSearchable() || this.isUpdating());

    ngOnInit(): void {
        const groupData = this.group();
        this.groupName.set(groupData.name);
        this.isPublic.set(groupData.isPublic);
        this.isSearchable.set(groupData.isSearchable);
    }

    onSearchableChange(value: boolean): void {
        // If searchable is unchecked, automatically set to private
        if (!value) {
            this.isPublic.set(false);
        }
    }

    validateGroupName(): boolean {
        this.validationError.set('');

        if (!this.groupName().trim()) {
            this.validationError.set('Group name is required');
            return false;
        }

        if (this.groupName().trim().length < 3) {
            this.validationError.set('Group name must be at least 3 characters long');
            return false;
        }

        if (this.groupName().trim().length > 100) {
            this.validationError.set('Group name must not exceed 100 characters');
            return false;
        }

        const alphanumericPattern = /^[a-zA-Z0-9\s]+$/;
        if (!alphanumericPattern.test(this.groupName())) {
            this.validationError.set('Group name can only contain alphanumeric characters and spaces');
            return false;
        }

        return true;
    }

    onSubmit(): void {
        if (!this.validateGroupName()) {
            return;
        }

        this.isUpdating.set(true);
        this.groupsService
            .updateGroup(this.group().id, {
                name: this.groupName().trim(),
                isPublic: this.isPublic(),
                isSearchable: this.isSearchable(),
            })
            .subscribe({
                next: (result) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: 'Success',
                        detail: `Group "${result.name}" updated successfully`,
                    });

                    this.isUpdating.set(false);
                    this.groupUpdated.emit();
                },
                error: (error) => {
                    this.isUpdating.set(false);
                    const errorMessage = error.error?.error || 'Failed to update group';
                    this.validationError.set(errorMessage);
                    this.messageService.add({
                        severity: 'error',
                        summary: 'Error',
                        detail: errorMessage,
                    });
                },
            });
    }
}
