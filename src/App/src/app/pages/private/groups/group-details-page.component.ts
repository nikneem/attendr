import { Component, inject, OnInit, ChangeDetectorRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { AllGroupsService } from '../../../shared/services/all-groups.service';
import { GroupDetailsDto } from '../../../shared/models/group-details-dto';

@Component({
    selector: 'attn-group-details-page',
    imports: [CommonModule, FormsModule, CardModule, ProgressSpinnerModule, TagModule, ButtonModule, SelectModule, TooltipModule],
    templateUrl: './group-details-page.component.html',
    styleUrl: './group-details-page.component.scss',
})
export class GroupDetailsPageComponent implements OnInit {
    private readonly groupsService = inject(AllGroupsService);
    private readonly route = inject(ActivatedRoute);
    private readonly cdr = inject(ChangeDetectorRef);

    group = signal<GroupDetailsDto | null>(null);
    loading = signal(true);
    error = signal<string | null>(null);

    roleOptions = [
        { label: 'Owner', value: 0 },
        { label: 'Manager', value: 1 },
        { label: 'Member', value: 2 }
    ];

    ngOnInit(): void {
        const groupId = this.route.snapshot.paramMap.get('id');
        if (groupId) {
            setTimeout(() => {
                this.loadGroupDetails(groupId);
                this.cdr.markForCheck();
            }, 0);
        } else {
            this.error.set('Invalid group ID');
            this.loading.set(false);
        }
    }

    private loadGroupDetails(id: string): void {
        this.loading.set(true);
        this.error.set(null);

        this.groupsService.getGroupDetails(id).subscribe({
            next: (group) => {
                this.group.set(group);
                this.loading.set(false);
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.error.set(err.message || 'Failed to load group details');
                this.loading.set(false);
                this.cdr.markForCheck();
            },
        });
    }

    isOwner(): boolean {
        return this.group()?.currentMemberRole === 0;
    }

    isOwnerOrManager(): boolean {
        const role = this.group()?.currentMemberRole;
        return role === 0 || role === 1;
    }

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
        console.log('Role change:', memberId, newRole);
        // TODO: Implement role change API call
        // this.groupsService.updateMemberRole(this.group()!.id, memberId, newRole).subscribe({
        //     next: () => this.loadGroupDetails(this.group()!.id),
        //     error: (err) => console.error('Failed to update role:', err)
        // });
    }

    approveJoinRequest(requestId: string): void {
        console.log('Approve request:', requestId);
        // TODO: Implement approve API call
        // this.groupsService.approveJoinRequest(this.group()!.id, requestId).subscribe({
        //     next: () => this.loadGroupDetails(this.group()!.id),
        //     error: (err) => console.error('Failed to approve request:', err)
        // });
    }

    declineJoinRequest(requestId: string): void {
        console.log('Decline request:', requestId);
        // TODO: Implement decline API call
        // this.groupsService.declineJoinRequest(this.group()!.id, requestId).subscribe({
        //     next: () => this.loadGroupDetails(this.group()!.id),
        //     error: (err) => console.error('Failed to decline request:', err)
        // });
    }

    cancelInvitation(invitationId: string): void {
        console.log('Cancel invitation:', invitationId);
        // TODO: Implement cancel invitation API call
        // this.groupsService.cancelInvitation(this.group()!.id, invitationId).subscribe({
        //     next: () => this.loadGroupDetails(this.group()!.id),
        //     error: (err) => console.error('Failed to cancel invitation:', err)
        // });
    }
}
