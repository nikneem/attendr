import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { AllGroupsStore } from '../../../shared/stores/all-groups.store';

@Component({
    selector: 'attn-groups-list-page',
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        CardModule,
        InputTextModule,
        PaginatorModule,
        ProgressSpinnerModule,
        TagModule,
    ],
    templateUrl: './groups-list-page.component.html',
    styleUrl: './groups-list-page.component.scss',
})
export class GroupsListPageComponent implements OnInit {
    private readonly groupsStore = inject(AllGroupsStore);
    private readonly cdr = inject(ChangeDetectorRef);

    groups = this.groupsStore.groups;
    loading = this.groupsStore.loading;
    error = this.groupsStore.error;
    totalCount = this.groupsStore.totalCount;
    pageSize = this.groupsStore.pageSize;
    pageNumber = this.groupsStore.pageNumber;
    searchQuery = this.groupsStore.searchQuery;

    searchInput = '';

    ngOnInit(): void {
        // Defer loading to avoid ExpressionChangedAfterItHasBeenCheckedError
        setTimeout(() => {
            this.groupsStore.loadGroups();
            this.cdr.markForCheck();
        }, 0);
    }

    onSearch(): void {
        this.groupsStore.search(this.searchInput);
    }

    onPageChange(event: PaginatorState): void {
        const pageNumber = (event.page ?? 0) + 1; // PrimeNG uses 0-based, API uses 1-based
        this.groupsStore.goToPage(pageNumber);
    }

    onClearSearch(): void {
        this.searchInput = '';
        this.groupsStore.search('');
    }
}
