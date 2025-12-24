import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { GroupDetailsStore } from '@stores/group-details.store';
import { ConferencesGroupJoinComponent } from '../conferences-group-join/conferences-group-join.component';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';

@Component({
    selector: 'attn-group-conferences',
    standalone: true,
    imports: [CommonModule, CardModule, TagModule, ButtonModule, DialogModule, ConferencesGroupJoinComponent, ConfirmationDialogComponent],
    templateUrl: './group-conferences.component.html',
    styleUrl: './group-conferences.component.scss',
})
export class GroupConferencesComponent {
    private readonly store = inject(GroupDetailsStore);

    group = this.store.groupDetails;
    conferences = computed(() => this.group()?.followedConferences ?? []);
    isOwnerOrManager = this.store.isOwnerOrManager;
    showJoinDialog = signal(false);
    showUnfollowDialog = signal(false);
    pendingConference = signal<{ id: string; name: string } | null>(null);
    unfollowMessage = computed(() => {
        const pending = this.pendingConference();
        return pending ? `Stop following ${pending.name}?` : '';
    });

    unfollow(conferenceId: string): void {
        const groupId = this.group()?.id;
        if (!groupId) return;
        this.store.unfollowConference(groupId, conferenceId);
    }

    openJoinDialog(): void {
        this.showJoinDialog.set(true);
    }

    closeJoinDialog(): void {
        this.showJoinDialog.set(false);
    }

    confirmUnfollow(conferenceId: string, conferenceName: string): void {
        this.pendingConference.set({ id: conferenceId, name: conferenceName });
        this.showUnfollowDialog.set(true);
    }

    onUnfollowConfirmed(): void {
        const pending = this.pendingConference();
        const groupId = this.group()?.id;
        if (!pending || !groupId) {
            this.resetUnfollowState();
            return;
        }

        this.store.unfollowConference(groupId, pending.id);
        this.resetUnfollowState();
    }

    onUnfollowCancelled(): void {
        this.resetUnfollowState();
    }

    onUnfollowDialogVisibilityChange(visible: boolean): void {
        if (!visible) {
            this.resetUnfollowState();
        }
    }

    private resetUnfollowState(): void {
        this.showUnfollowDialog.set(false);
        this.pendingConference.set(null);
    }
}
