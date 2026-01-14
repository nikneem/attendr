import { Component, computed, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { GroupDetailsStore } from '@stores/group-details.store';

@Component({
    selector: 'attn-group-members',
    standalone: true,
    imports: [CommonModule, FormsModule, CardModule, TagModule, ButtonModule, SelectModule, TooltipModule],
    templateUrl: './group-members.component.html',
    styleUrl: './group-members.component.scss',
})
export class GroupMembersComponent {
    private readonly store = inject(GroupDetailsStore);
    private readonly messageService = inject(MessageService);
    private readonly confirmationService = inject(ConfirmationService);

    groupId = input.required<string>();

    group = this.store.groupDetails;
    isOwner = this.store.isOwner;
    isOwnerOrManager = this.store.isOwnerOrManager;

    members = computed(() => this.group()?.members ?? []);

    roleOptions = [
        { label: 'Manager', value: 1 },
        { label: 'Member', value: 2 }
    ];

    getRoleName(role: number): string {
        switch (role) {
            case 0: return 'Owner';
            case 1: return 'Manager';
            case 2: return 'Member';
            default: return 'Unknown';
        }
    }

    getRoleSeverity(role: number): 'success' | 'info' | 'secondary' {
        switch (role) {
            case 0: return 'success';
            case 1: return 'info';
            case 2: return 'secondary';
            default: return 'secondary';
        }
    }

    onRoleChange(memberId: string, newRole: number): void {
        const newRoleName = this.getRoleName(newRole);

        this.confirmationService.confirm({
            message: `Do you want to change the member role to ${newRoleName}?`,
            header: 'Change Member Role',
            icon: 'pi pi-exclamation-triangle',
            acceptIcon: 'pi pi-check',
            rejectIcon: 'pi pi-times',
            acceptLabel: 'Yes, Change Role',
            rejectLabel: 'Cancel',
            accept: () => {
                this.store.updateMemberRole(this.groupId(), memberId, newRole);

                // Show success message if no error occurred
                setTimeout(() => {
                    if (!this.store.error()) {
                        this.messageService.add({
                            severity: 'success',
                            summary: 'Success',
                            detail: 'Member role updated successfully',
                        });
                    } else {
                        this.messageService.add({
                            severity: 'error',
                            summary: 'Error',
                            detail: this.store.error() || 'Failed to update member role',
                        });
                        this.store.clearError();
                    }
                }, 500);
            },
            reject: () => {
                // Reload to reset the dropdown to the original value
                this.store.loadGroupDetails(this.groupId());
            }
        });
    }

    canRemoveMember(memberRole: number): boolean {
        // Only owners and managers can remove members
        if (!this.isOwnerOrManager()) {
            return false;
        }

        // Can't remove owners
        return memberRole !== 0;
    }

    removeMember(memberId: string): void {
        this.confirmationService.confirm({
            message: 'Are you sure you want to remove this member from the group?',
            header: 'Remove Member',
            icon: 'pi pi-exclamation-triangle',
            acceptIcon: 'pi pi-check',
            rejectIcon: 'pi pi-times',
            acceptLabel: 'Yes, Remove',
            rejectLabel: 'Cancel',
            accept: () => {
                this.store.removeMember(this.groupId(), memberId);

                // Show success message if no error occurred
                setTimeout(() => {
                    if (!this.store.error()) {
                        this.messageService.add({
                            severity: 'success',
                            summary: 'Success',
                            detail: 'Member removed successfully',
                        });
                    } else {
                        this.messageService.add({
                            severity: 'error',
                            summary: 'Error',
                            detail: this.store.error() || 'Failed to remove member',
                        });
                        this.store.clearError();
                    }
                }, 500);
            }
        });
    }
}
