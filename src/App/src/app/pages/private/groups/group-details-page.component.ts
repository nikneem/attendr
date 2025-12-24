import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';
import { GroupDetailsStore } from '../../../shared/stores/group-details.store';
import { GroupMembersComponent } from '../../../shared/components/group-members/group-members.component';
import { GroupConferencesComponent } from '../../../shared/components/group-conferences/group-conferences.component';
import { GroupJoinRequestsComponent } from '../../../shared/components/group-join-requests/group-join-requests.component';
import { GroupInvitationsComponent } from '../../../shared/components/group-invitations/group-invitations.component';

@Component({
    selector: 'attn-group-details-page',
    imports: [
        CommonModule,
        ProgressSpinnerModule,
        TagModule,
        ButtonModule,
        GroupMembersComponent,
        GroupConferencesComponent,
        GroupJoinRequestsComponent,
        GroupInvitationsComponent,
    ],
    templateUrl: './group-details-page.component.html',
    styleUrl: './group-details-page.component.scss',
})
export class GroupDetailsPageComponent implements OnInit {
    private readonly store = inject(GroupDetailsStore);
    private readonly messageService = inject(MessageService);
    private readonly route = inject(ActivatedRoute);

    groupId = signal<string>('');
    group = this.store.groupDetails;
    loading = this.store.loading;
    error = this.store.error;
    joiningGroup = signal(false);

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.groupId.set(id);
            this.store.loadGroupDetails(id);
        }
    }

    joinGroup(): void {
        const groupId = this.group()?.id;
        if (!groupId || this.joiningGroup()) {
            return;
        }

        this.joiningGroup.set(true);

        this.groupsService.joinGroup(groupId).subscribe({
            next: () => {
                this.loadGroupDetails(groupId);
                this.joiningGroup.set(false);
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Successfully joined the group',
                });
            },
            error: (err) => {
                console.error('Failed to join group:', err);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: err.error?.error || 'Failed to join group. Please try again.',
                });
                this.joiningGroup.set(false);
            }
        });
    }
}
id = this.groupId();
if (!id || this.joiningGroup()) {
    return;
}

this.joiningGroup.set(true);
this.store.joinGroup(id);

// Check for success after a brief delay
setTimeout(() => {
    this.joiningGroup.set(false);
    if (!this.store.error()) {
        this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Successfully joined the group',
        });
    } else {
        this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: this.store.error() || 'Failed to join group',
        });
        this.store.clearError();
    }
}, 500