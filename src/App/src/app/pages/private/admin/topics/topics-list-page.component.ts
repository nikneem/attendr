import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { finalize } from 'rxjs';
import { TopicsService, TopicDto } from '../../../../services/topics.service';

@Component({
    selector: 'attn-topics-list-page',
    standalone: true,
    imports: [CommonModule, TableModule, ButtonModule, ProgressSpinnerModule, TooltipModule, TagModule],
    templateUrl: './topics-list-page.component.html',
    styleUrl: './topics-list-page.component.scss',
})
export class TopicsListPageComponent implements OnInit {
    private readonly topicsService = inject(TopicsService);

    topics: TopicDto[] = [];
    loading = true;
    error: string | null = null;

    ngOnInit(): void {
        this.loadTopics();
    }

    private loadTopics(): void {
        this.loading = true;
        this.error = null;

        this.topicsService
            .getAllTopics()
            .pipe(finalize(() => (this.loading = false)))
            .subscribe({
                next: (topics) => {
                    this.topics = topics;
                },
                error: (err) => {
                    this.error = err.message || 'Failed to load topics';
                    console.error('Error loading topics:', err);
                },
            });
    }

    getVisibilityLabel(isVisible: boolean): string {
        return isVisible ? 'Visible' : 'Hidden';
    }

    getVisibilitySeverity(isVisible: boolean): 'success' | 'warn' {
        return isVisible ? 'success' : 'warn';
    }
}
