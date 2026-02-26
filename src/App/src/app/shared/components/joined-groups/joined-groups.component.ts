import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { JoinedGroupsStore } from '@stores/joined-groups.store';
import { AllGroupsComponent } from '@components/all-groups/all-groups.component';
import { CreateGroupComponent } from '@components/create-group/create-group.component';

import { TranslateModule } from '@ngx-translate/core';
@Component({
    selector: 'attn-joined-groups',
    imports: [CommonModule, ButtonModule, TableModule, CardModule, DialogModule, AllGroupsComponent, CreateGroupComponent,
        TranslateModule
    ],
    templateUrl: './joined-groups.component.html',
    styleUrl: './joined-groups.component.scss',
})
export class JoinedGroupsComponent implements OnInit {
    readonly store = inject(JoinedGroupsStore);
    private readonly router = inject(Router);
    showJoinGroupDialog = false;
    showCreateGroupDialog = false;

    ngOnInit(): void {
        this.store.loadGroups();
    }

    navigateToGroup(groupId: string): void {
        this.router.navigate(['/app/groups', groupId]);
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
