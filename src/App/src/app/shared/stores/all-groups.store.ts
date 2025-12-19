import { Injectable, signal, inject, computed } from '@angular/core';
import { GroupListItemDto } from '../models/group-list-item-dto';
import { AllGroupsService } from '../services/all-groups.service';

@Injectable({
    providedIn: 'root',
})
export class AllGroupsStore {
    private readonly allGroupsService = inject(AllGroupsService);

    private readonly _groups = signal<GroupListItemDto[]>([]);
    private readonly _loading = signal<boolean>(false);
    private readonly _error = signal<string | null>(null);
    private readonly _totalCount = signal<number>(0);
    private readonly _pageSize = signal<number>(25);
    private readonly _pageNumber = signal<number>(1);
    private readonly _searchQuery = signal<string>('');

    readonly groups = this._groups.asReadonly();
    readonly loading = this._loading.asReadonly();
    readonly error = this._error.asReadonly();
    readonly totalCount = this._totalCount.asReadonly();
    readonly pageSize = this._pageSize.asReadonly();
    readonly pageNumber = this._pageNumber.asReadonly();
    readonly searchQuery = this._searchQuery.asReadonly();

    readonly totalPages = computed(() => Math.ceil(this._totalCount() / this._pageSize()));

    loadGroups(searchQuery?: string, pageSize?: number, pageNumber?: number): void {
        this._loading.set(true);
        this._error.set(null);

        // Update state
        if (searchQuery !== undefined) {
            this._searchQuery.set(searchQuery);
        }
        if (pageSize !== undefined) {
            this._pageSize.set(pageSize);
        }
        if (pageNumber !== undefined) {
            this._pageNumber.set(pageNumber);
        }

        this.allGroupsService
            .listGroups(
                this._searchQuery() || undefined,
                this._pageSize(),
                this._pageNumber()
            )
            .subscribe({
                next: (result) => {
                    this._groups.set(result.groups);
                    this._totalCount.set(result.totalCount);
                    this._pageSize.set(result.pageSize);
                    this._pageNumber.set(result.pageNumber);
                    this._loading.set(false);
                },
                error: (err) => {
                    this._error.set(err.message || 'Failed to load groups');
                    this._loading.set(false);
                },
            });
    }

    search(query: string): void {
        this.loadGroups(query, this._pageSize(), 1); // Reset to page 1 on search
    }

    goToPage(pageNumber: number): void {
        if (pageNumber >= 1 && pageNumber <= this.totalPages()) {
            this.loadGroups(this._searchQuery(), this._pageSize(), pageNumber);
        }
    }

    refresh(): void {
        this.loadGroups(this._searchQuery(), this._pageSize(), this._pageNumber());
    }
}
