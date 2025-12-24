import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { GroupDetailsStore } from '@stores/group-details.store';
import { ConferencesGroupJoinComponent } from '../conferences-group-join/conferences-group-join.component';

@Component({
    selector: 'attn-group-conferences',
    standalone: true,
    imports: [CommonModule, CardModule, TagModule, ButtonModule, DialogModule, ConferencesGroupJoinComponent],
    templateUrl: './group-conferences.component.html',
    styleUrl: './group-conferences.component.scss',
})
export class GroupConferencesComponent {
    private readonly store = inject(GroupDetailsStore);

    group = this.store.groupDetails;
    conferences = computed(() => this.group()?.followedConferences ?? []);
    isOwnerOrManager = this.store.isOwnerOrManager;
    showJoinDialog = signal(false);

    openJoinDialog(): void {
        this.showJoinDialog.set(true);
    }

    closeJoinDialog(): void {
        this.showJoinDialog.set(false);
    }
}
