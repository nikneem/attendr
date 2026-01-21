import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { finalize } from 'rxjs';
import { TopicsService, TopicDto } from '../../../../shared/services/topics.service';

@Component({
    selector: 'attn-topics-list-page',
    standalone: true,
    imports: [CommonModule, TableModule, ButtonModule, ProgressSpinnerModule, TooltipModule, TagModule],
    templateUrl: './topics-list-page.component.html',
    styleUrl: './topics-list-page.component.scss',
})
export class TopicsListPageComponent implements OnInit {
    private readonly topicsService = inject(TopicsService);

    protected readonly topics = signal<TopicDto[]>([]);
    protected readonly loading = signal<boolean>(true);
    protected readonly error = signal<string | null>(null);

    ngOnInit(): void {
        this.loadTopics();
    }

    private loadTopics(): void {
        this.loading.set(true);
        this.error.set(null);

        this.topicsService
            .getAllTopics()
            .pipe(finalize(() => this.loading.set(false)))
            .subscribe({
                next: (topics) => {
                    this.topics.set(topics);
                },
                error: (err) => {
                    this.error.set(err.message || 'Failed to load topics');
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
