import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { JoinedGroupsStore } from '../../stores/joined-groups.store';
import { AllGroupsComponent } from '../all-groups/all-groups.component';
import { CreateGroupComponent } from '../create-group/create-group.component';

@Component({
    selector: 'attn-joined-groups',
    imports: [CommonModule, ButtonModule, TableModule, CardModule, DialogModule, AllGroupsComponent, CreateGroupComponent],
    templateUrl: './joined-groups.component.html',
    styleUrl: './joined-groups.component.scss',
})
export class JoinedGroupsComponent implements OnInit {
    readonly store = inject(JoinedGroupsStore);
    showJoinGroupDialog = false;
    showCreateGroupDialog = false;

    ngOnInit(): void {
        this.store.loadGroups();
    }

    onCreateGroup(): void {
        this.showCreateGroupDialog = true;
    }

    onJoinGroup(): void {
        this.showJoinGroupDialog = true;
    }

    onCreateDialogHide(): void {
        this.showCreateGroupDialog = false;
    }

    onJoinDialogHide(): void {
        this.showJoinGroupDialog = false;
        // Refresh the list of joined groups when the dialog closes
        this.store.refresh();
    }

    onGroupCreated(group: { id: string; name: string; memberCount: number }): void {
        // Add the new group to the store
        this.store.addGroup(group);
        // Close the dialog
        this.showCreateGroupDialog = false;
    }
}
