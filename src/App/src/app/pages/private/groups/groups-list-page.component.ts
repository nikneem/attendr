import { Component, inject, OnInit, ChangeDetectorRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { AllGroupsStore } from '../../../shared/stores/all-groups.store';
import { GroupListItemDto } from '../../../shared/models/group-list-item-dto';
import { GroupDetailsViewComponent } from '../../../shared/components/group-details-view/group-details-view.component';

@Component({
    selector: 'attn-groups-list-page',
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        CardModule,
        DialogModule,
        InputTextModule,
        PaginatorModule,
        ProgressSpinnerModule,
        TagModule,
        GroupDetailsViewComponent,
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
    showDetailsDialog = signal(false);
    selectedGroup = signal<GroupListItemDto | null>(null);

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

    viewGroupDetails(group: GroupListItemDto): void {
        this.selectedGroup.set(group);
        this.showDetailsDialog.set(true);
    }

    onJoinGroup(groupId: string): void {
        console.log('Join group:', groupId);
        // TODO: Implement join group functionality
        this.showDetailsDialog.set(false);
    }

    onRequestAccess(groupId: string): void {
        console.log('Request access to group:', groupId);
        // TODO: Implement request access functionality
        this.showDetailsDialog.set(false);
    }
}
