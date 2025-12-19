import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { PaginatorModule } from 'primeng/paginator';
import { AllGroupsStore } from '../../stores/all-groups.store';

@Component({
    selector: 'attn-all-groups',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        TableModule,
        CardModule,
        InputTextModule,
        TagModule,
        PaginatorModule,
    ],
    templateUrl: './all-groups.component.html',
    styleUrl: './all-groups.component.scss',
})
export class AllGroupsComponent implements OnInit {
    readonly store = inject(AllGroupsStore);
    searchQuery = '';

    ngOnInit(): void {
        this.store.loadGroups();
    }

    onSearch(): void {
        this.store.search(this.searchQuery);
    }

    onSearchClear(): void {
        this.searchQuery = '';
        this.store.search('');
    }

    onPageChange(event: any): void {
        const pageNumber = event.page + 1; // PrimeNG uses 0-based page index
        this.store.goToPage(pageNumber);
    }

    onJoinGroup(groupId: string): void {
        // TODO: Implement join group logic
        console.log('Join group:', groupId);
    }
}
