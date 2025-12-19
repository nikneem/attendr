import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { JoinedGroupsStore } from '../../stores/joined-groups.store';

@Component({
    selector: 'attn-joined-groups',
    imports: [CommonModule, ButtonModule, TableModule, CardModule],
    templateUrl: './joined-groups.component.html',
    styleUrl: './joined-groups.component.scss',
})
export class JoinedGroupsComponent implements OnInit {
    readonly store = inject(JoinedGroupsStore);

    ngOnInit(): void {
        this.store.loadGroups();
    }

    onCreateGroup(): void {
        // TODO: Implement create group navigation
        console.log('Create group clicked');
    }

    onJoinGroup(): void {
        // TODO: Implement join group navigation
        console.log('Join group clicked');
    }
}
