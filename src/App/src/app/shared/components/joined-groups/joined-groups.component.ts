import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { JoinedGroupsStore } from '../../stores/joined-groups.store';
import { AllGroupsComponent } from '../all-groups/all-groups.component';

@Component({
    selector: 'attn-joined-groups',
    imports: [CommonModule, ButtonModule, TableModule, CardModule, DialogModule, AllGroupsComponent],
    templateUrl: './joined-groups.component.html',
    styleUrl: './joined-groups.component.scss',
})
export class JoinedGroupsComponent implements OnInit {
    readonly store = inject(JoinedGroupsStore);
    showJoinGroupDialog = false;

    ngOnInit(): void {
        this.store.loadGroups();
    }

    onCreateGroup(): void {
        // TODO: Implement create group navigation
        console.log('Create group clicked');
    }

    onJoinGroup(): void {
        this.showJoinGroupDialog = true;
    }

    onDialogHide(): void {
        this.showJoinGroupDialog = false;
        // Refresh the list of joined groups when the dialog closes
        this.store.refresh();
    }
}
