import { Component, computed, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { GroupDetailsStore } from '../../stores/group-details.store';

@Component({
    selector: 'attn-group-invitations',
    standalone: true,
    imports: [CommonModule, CardModule, TagModule, ButtonModule, TooltipModule],
    templateUrl: './group-invitations.component.html',
    styleUrl: './group-invitations.component.scss',
})
export class GroupInvitationsComponent {
    private readonly store = inject(GroupDetailsStore);
    private readonly messageService = inject(MessageService);

    groupId = input.required<string>();

    group = this.store.groupDetails;
    isOwner = this.store.isOwner;
    invitations = computed(() => this.group()?.invitations ?? []);

    cancelInvitation(invitationId: string): void {
        this.store.cancelInvitation(this.groupId(), invitationId);

        // Show success message if no error occurred
        setTimeout(() => {
            if (!this.store.error()) {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Invitation cancelled',
                });
            } else {
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: this.store.error() || 'Failed to cancel invitation',
                });
                this.store.clearError();
            }
        }, 500);
    }
}
