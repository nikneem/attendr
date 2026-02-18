import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { finalize } from 'rxjs';
import { MessageService } from 'primeng/api';
import { TopicsService, TopicDto } from '../../../../shared/services/topics.service';
import { EditTopicComponent } from '../../../../shared/components/edit-topic/edit-topic.component';
import { CreateTopicComponent } from '../../../../shared/components/create-topic/create-topic.component';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';

@Component({
    selector: 'attn-topics-list-page',
    standalone: true,
    imports: [CommonModule, TableModule, ButtonModule, ProgressSpinnerModule, TooltipModule, TagModule, DialogModule, EditTopicComponent, CreateTopicComponent, ConfirmationDialogComponent],
    templateUrl: './topics-list-page.component.html',
    styleUrl: './topics-list-page.component.scss',
})
export class TopicsListPageComponent implements OnInit {
    private readonly topicsService = inject(TopicsService);
    private readonly messageService = inject(MessageService);

    @ViewChild('createTopicRef') private createTopicRef?: CreateTopicComponent;

    protected readonly topics = signal<TopicDto[]>([]);
    protected readonly loading = signal<boolean>(true);
    protected readonly error = signal<string | null>(null);

    protected readonly createDialogVisible = signal<boolean>(false);
    protected readonly editDialogVisible = signal<boolean>(false);
    protected readonly confirmDeleteVisible = signal<boolean>(false);
    protected readonly selectedTopic = signal<TopicDto | null>(null);

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

    getVisibilityIcon(isVisible: boolean): string {
        return isVisible ? 'Y' : 'X';
    }

    getVisibilitySeverity(isVisible: boolean): 'success' | 'warn' {
        return isVisible ? 'success' : 'warn';
    }

    openCreate(): void {
        this.createTopicRef?.reset();
        this.createDialogVisible.set(true);
    }

    onCreateCancel(): void {
        this.createDialogVisible.set(false);
    }

    onCreateSave(payload: { key: string; name: string }): void {
        this.topicsService.createTopic(payload.key, payload.name).subscribe({
            next: (created) => {
                this.topics.set([...this.topics(), created]);
                this.createDialogVisible.set(false);
                this.messageService.add({ severity: 'success', summary: 'Topic created', detail: created.name });
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'Create failed', detail: err.message || 'Failed to create topic' });
            },
        });
    }

    openEdit(topic: TopicDto): void {
        this.selectedTopic.set(topic);
        this.editDialogVisible.set(true);
    }

    onEditCancel(): void {
        this.editDialogVisible.set(false);
        this.selectedTopic.set(null);
    }

    onEditSave(payload: { key: string; name: string; isVisible: boolean }): void {
        const current = this.selectedTopic();
        if (!current) return;
        this.topicsService
            .updateTopic(current.id, payload.key, payload.name, payload.isVisible)
            .subscribe({
                next: (updated) => {
                    // Update list in-place
                    const updatedList = this.topics().map((t) =>
                        t.id === updated.id ? { ...t, ...updated } : t
                    );
                    this.topics.set(updatedList);
                    this.onEditCancel();
                    this.messageService.add({ severity: 'success', summary: 'Topic updated', detail: updated.name });
                },
                error: (err) => {
                    this.error.set(err.message || 'Failed to update topic');
                    this.messageService.add({ severity: 'error', summary: 'Update failed', detail: err.message || 'Failed to update topic' });
                },
            });
    }

    onEditDelete(): void {
        this.confirmDeleteVisible.set(true);
    }

    onConfirmDelete(): void {
        const current = this.selectedTopic();
        if (!current) return;
        this.topicsService.deleteTopic(current.id).subscribe({
            next: () => {
                const updatedList = this.topics().filter((t) => t.id !== current.id);
                this.topics.set(updatedList);
                this.confirmDeleteVisible.set(false);
                this.onEditCancel();
                this.messageService.add({ severity: 'success', summary: 'Topic deleted', detail: current.name });
            },
            error: (err) => {
                this.error.set(err.message || 'Failed to delete topic');
                this.confirmDeleteVisible.set(false);
                this.messageService.add({ severity: 'error', summary: 'Delete failed', detail: err.message || 'Failed to delete topic' });
            },
        });
    }

    onCancelDelete(): void {
        this.confirmDeleteVisible.set(false);
    }

    toggleVisibility(topic: TopicDto): void {
        const newVisibility = !topic.isVisible;
        this.topicsService.updateTopic(topic.id, topic.key, topic.name, newVisibility).subscribe({
            next: (updated) => {
                const updatedList = this.topics().map((t) =>
                    t.id === updated.id ? { ...t, isVisible: updated.isVisible } : t
                );
                this.topics.set(updatedList);
                this.messageService.add({ severity: 'success', summary: 'Visibility updated', detail: updated.isVisible ? 'Topic is now visible' : 'Topic is now hidden' });
            },
            error: (err) => {
                this.error.set(err.message || 'Failed to update visibility');
                this.messageService.add({ severity: 'error', summary: 'Visibility update failed', detail: err.message || 'Failed to update visibility' });
            },
        });
    }
}
