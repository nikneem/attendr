import { Component, computed, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { GroupDetailsStore } from '@stores/group-details.store';

@Component({
    selector: 'attn-group-join-requests',
    standalone: true,
    imports: [CommonModule, CardModule, TagModule, ButtonModule, TooltipModule],
    templateUrl: './group-join-requests.component.html',
    styleUrl: './group-join-requests.component.scss',
})
export class GroupJoinRequestsComponent {
    private readonly store = inject(GroupDetailsStore);
    private readonly messageService = inject(MessageService);

    groupId = input.required<string>();

    group = this.store.groupDetails;
    isOwnerOrManager = this.store.isOwnerOrManager;
    joinRequests = computed(() => this.group()?.joinRequests ?? []);

    approveJoinRequest(requestId: string): void {
        this.store.approveJoinRequest(this.groupId(), requestId);

        // Show success message if no error occurred
        setTimeout(() => {
            if (!this.store.error()) {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Join request approved',
                });
            } else {
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: this.store.error() || 'Failed to approve join request',
                });
                this.store.clearError();
            }
        }, 500);
    }

    declineJoinRequest(requestId: string): void {
        this.store.denyJoinRequest(this.groupId(), requestId);

        // Show success message if no error occurred
        setTimeout(() => {
            if (!this.store.error()) {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Join request declined',
                });
            } else {
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: this.store.error() || 'Failed to decline join request',
                });
                this.store.clearError();
            }
        }, 500);
    }
}
