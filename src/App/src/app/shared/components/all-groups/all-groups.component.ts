import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { TagModule } from 'primeng/tag';
import { PaginatorModule } from 'primeng/paginator';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { AllGroupsStore } from '../../stores/all-groups.store';
import { AllGroupsService } from '../../services/all-groups.service';

import { TranslateModule } from '@ngx-translate/core';
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
        IconFieldModule,
        InputIconModule,
        TagModule,
        PaginatorModule,
        TranslateModule
    ],
    templateUrl: './all-groups.component.html',
    styleUrl: './all-groups.component.scss',
})
export class AllGroupsComponent implements OnInit, OnDestroy {
    readonly store = inject(AllGroupsStore);
    private readonly groupsService = inject(AllGroupsService);
    private readonly messageService = inject(MessageService);
    searchQuery = '';
    private searchSubject = new Subject<string>();

    ngOnInit(): void {
        this.store.loadGroups();

        // Setup debounced search
        this.searchSubject
            .pipe(
                debounceTime(500),
                distinctUntilChanged()
            )
            .subscribe(query => {
                this.store.search(query);
            });
    }

    ngOnDestroy(): void {
        this.searchSubject.complete();
    }

    onSearchInput(): void {
        this.searchSubject.next(this.searchQuery);
    }

    onSearchKeyup(event: KeyboardEvent): void {
        if (event.key === 'Enter') {
            this.store.search(this.searchQuery);
        }
    }

    onSearchClear(): void {
        this.searchQuery = '';
        this.searchSubject.next('');
    }

    onPageChange(event: any): void {
        const pageNumber = event.page + 1; // PrimeNG uses 0-based page index
        this.store.goToPage(pageNumber);
    }

    onJoinGroup(groupId: string): void {
        this.groupsService.joinGroup(groupId).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Successfully joined the group',
                });
                // Refresh the groups list to show updated status
                this.store.loadGroups(
                    this.searchQuery || undefined,
                    this.store.pageSize(),
                    this.store.pageNumber()
                );
            },
            error: (err: any) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: err.error?.error || 'Failed to join group. Please try again.',
                });
            },
        });
    }
}
