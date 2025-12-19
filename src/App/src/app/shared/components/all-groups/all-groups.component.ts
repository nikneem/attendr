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
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
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
        IconFieldModule,
        InputIconModule,
        TagModule,
        PaginatorModule,
    ],
    templateUrl: './all-groups.component.html',
    styleUrl: './all-groups.component.scss',
})
export class AllGroupsComponent implements OnInit, OnDestroy {
    readonly store = inject(AllGroupsStore);
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
        // TODO: Implement join group logic
        console.log('Join group:', groupId);
    }
}
