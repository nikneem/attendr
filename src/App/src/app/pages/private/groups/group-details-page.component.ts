import { Component, inject, OnInit, ChangeDetectorRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { AllGroupsService } from '../../../shared/services/all-groups.service';
import { GroupDetailsDto } from '../../../shared/models/group-details-dto';

@Component({
    selector: 'attn-group-details-page',
    imports: [CommonModule, CardModule, ProgressSpinnerModule, TagModule],
    templateUrl: './group-details-page.component.html',
    styleUrl: './group-details-page.component.scss',
})
export class GroupDetailsPageComponent implements OnInit {
    private readonly groupsService = inject(AllGroupsService);
    private readonly route = inject(ActivatedRoute);
    private readonly cdr = inject(ChangeDetectorRef);

    group = signal<GroupDetailsDto | null>(null);
    loading = signal(true);
    error = signal<string | null>(null);

    ngOnInit(): void {
        const groupId = this.route.snapshot.paramMap.get('id');
        if (groupId) {
            setTimeout(() => {
                this.loadGroupDetails(groupId);
                this.cdr.markForCheck();
            }, 0);
        } else {
            this.error.set('Invalid group ID');
            this.loading.set(false);
        }
    }

    private loadGroupDetails(id: string): void {
        this.loading.set(true);
        this.error.set(null);

        this.groupsService.getGroupDetails(id).subscribe({
            next: (group) => {
                this.group.set(group);
                this.loading.set(false);
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.error.set(err.message || 'Failed to load group details');
                this.loading.set(false);
                this.cdr.markForCheck();
            },
        });
    }
}
