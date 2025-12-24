import { Component, computed, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
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

    groupId = input.required<string>();

    group = this.store.groupDetails;
    isOwner = this.store.isOwner;
    isOwnerOrManager = this.store.isOwnerOrManager;

    members = computed(() => this.group()?.members ?? []);

    roleOptions = [
        { label: 'Owner', value: 0 },
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
        this.store.updateMemberRole(this.groupId(), memberId, newRole);
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
        if (!confirm('Are you sure you want to remove this member from the group?')) {
            return;
        }

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
}
