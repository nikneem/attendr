import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ProgressBarModule } from 'primeng/progressbar';
import { MessageService } from 'primeng/api';
import { ProfileService } from '@services/profile.service';
import { ProfileTopicDto } from '@models/profile-topic-dto';

interface TopicWithRelativeTime extends ProfileTopicDto {
    relativeTime: string;
}

@Component({
    selector: 'attn-focus-areas-page',
    standalone: true,
    imports: [
        CommonModule,
        CardModule,
        ProgressSpinnerModule,
        ProgressBarModule,
    ],
    templateUrl: './focus-areas-page.component.html',
    styleUrl: './focus-areas-page.component.scss',
})
export class FocusAreasPageComponent implements OnInit {
    private readonly profileService = inject(ProfileService);
    private readonly messageService = inject(MessageService);

    protected topicsLoading = signal<boolean>(false);
    private rawTopics = signal<ProfileTopicDto[]>([]);

    protected topics = computed<TopicWithRelativeTime[]>(() => {
        return this.rawTopics().map(topic => ({
            ...topic,
            relativeTime: this.getRelativeTime(topic.createdOn)
        }));
    });

    ngOnInit(): void {
        this.loadTopics();
    }

    private loadTopics(): void {
        this.topicsLoading.set(true);

        this.profileService.getProfileTopics().subscribe({
            next: (topics) => {
                this.rawTopics.set(topics);
                this.topicsLoading.set(false);
            },
            error: (err) => {
                console.error('Failed to load topics', err);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to load focus areas.',
                });
                this.topicsLoading.set(false);
            },
        });
    }

    protected getRelativeTime(timestamp: string): string {
        const now = new Date();
        const date = new Date(timestamp);
        const diff = now.getTime() - date.getTime();
        const minutes = Math.floor(diff / (1000 * 60));
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const days = Math.floor(diff / (1000 * 60 * 60 * 24));

        if (minutes < 1) return 'Just now';
        if (minutes < 60) return `${minutes}m ago`;
        if (hours < 24) return `${hours}h ago`;
        if (days < 7) return `${days}d ago`;
        return date.toLocaleDateString();
    }
}
