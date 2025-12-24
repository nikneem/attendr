import { Injectable, signal, inject, computed } from '@angular/core';
import { GroupDetailsDto } from '../models/group-details-dto';
import { AllGroupsService } from '../services/all-groups.service';

@Injectable({
    providedIn: 'root',
})
export class GroupDetailsStore {
    private readonly groupsService = inject(AllGroupsService);

    private readonly _groupDetails = signal<GroupDetailsDto | null>(null);
    private readonly _loading = signal<boolean>(false);
    private readonly _error = signal<string | null>(null);

    readonly groupDetails = this._groupDetails.asReadonly();
    readonly loading = this._loading.asReadonly();
    readonly error = this._error.asReadonly();

    readonly isOwner = computed(() => this._groupDetails()?.currentMemberRole === 0);
    readonly isOwnerOrManager = computed(() => {
        const role = this._groupDetails()?.currentMemberRole;
        return role === 0 || role === 1;
    });
    readonly isMember = computed(() => this._groupDetails()?.isMember ?? false);

    loadGroupDetails(groupId: string): void {
        this._loading.set(true);
        this._error.set(null);

        this.groupsService.getGroupDetails(groupId).subscribe({
            next: (group) => {
                this._groupDetails.set(group);
                this._loading.set(false);
            },
            error: (err) => {
                this._error.set(err.message || 'Failed to load group details');
                this._loading.set(false);
            },
        });
    }

    updateMemberRole(groupId: string, memberId: string, newRole: number): void {
        // TODO: Implement when API is ready
        console.log('Update member role:', memberId, newRole);
        // After successful API call:
        // this.loadGroupDetails(groupId);
    }

    removeMember(groupId: string, memberId: string): void {
        this.groupsService.removeMember(groupId, memberId).subscribe({
            next: () => {
                this.loadGroupDetails(groupId);
            },
            error: (err) => {
                this._error.set(err.error?.error || 'Failed to remove member');
            },
        });
    }

    approveJoinRequest(groupId: string, requestId: string): void {
        this.groupsService.approveJoinRequest(groupId, requestId).subscribe({
            next: () => {
                this.loadGroupDetails(groupId);
            },
            error: (err) => {
                this._error.set(err.error?.error || 'Failed to approve join request');
            },
        });
    }

    denyJoinRequest(groupId: string, requestId: string): void {
        this.groupsService.denyJoinRequest(groupId, requestId).subscribe({
            next: () => {
                this.loadGroupDetails(groupId);
            },
            error: (err) => {
                this._error.set(err.error?.error || 'Failed to deny join request');
            },
        });
    }

    cancelInvitation(groupId: string, invitationId: string): void {
        // TODO: Implement when API is ready
        console.log('Cancel invitation:', invitationId);
        // After successful API call:
        // this.loadGroupDetails(groupId);
    }

    joinGroup(groupId: string): void {
        this.groupsService.joinGroup(groupId).subscribe({
            next: () => {
                this.loadGroupDetails(groupId);
            },
            error: (err) => {
                this._error.set(err.error?.error || 'Failed to join group');
            },
        });
    }

    followConference(groupId: string, conferenceId: string): void {
        this.groupsService.followConference(groupId, conferenceId).subscribe({
            next: () => {
                this.loadGroupDetails(groupId);
            },
            error: (err) => {
                this._error.set(err.error?.error || 'Failed to follow conference');
            },
        });
    }

    clearError(): void {
        this._error.set(null);
    }

    clear(): void {
        this._groupDetails.set(null);
        this._loading.set(false);
        this._error.set(null);
    }
}
