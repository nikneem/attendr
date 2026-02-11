import { Component, OnInit, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ProgressBarModule } from 'primeng/progressbar';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { PopoverModule } from 'primeng/popover';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { ProfileService } from '@services/profile.service';
import { ProfileTopicDto } from '@models/profile-topic-dto';
import { driver } from 'driver.js';

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
        TagModule,
        ButtonModule,
        PopoverModule,
        TooltipModule,
    ],
    templateUrl: './focus-areas-page.component.html',
    styleUrl: './focus-areas-page.component.scss',
})
export class FocusAreasPageComponent implements OnInit {
    private readonly profileService = inject(ProfileService);
    private readonly messageService = inject(MessageService);
    private readonly TOUR_COOKIE_NAME = 'focusAreasTourCompleted';

    protected topicsLoading = signal<boolean>(false);
    private rawTopics = signal<ProfileTopicDto[]>([]);

    protected topics = computed<TopicWithRelativeTime[]>(() => {
        return this.rawTopics()
            .map(topic => ({
                ...topic,
                relativeTime: this.getRelativeTime(topic.createdOn)
            }))
            .sort((a, b) => b.weight - a.weight);
    });

    constructor() {
        // Start tour when topics are loaded for the first time
        effect(() => {
            const topicsData = this.topics();
            const loading = this.topicsLoading();
            
            if (!loading && topicsData.length > 0 && !this.hasTourBeenShown()) {
                // Small delay to ensure DOM is rendered
                setTimeout(() => {
                    this.startTour();
                }, 500);
            }
        });
    }

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

    protected toggleTopicManualStatus(topic: ProfileTopicDto, event: Event): void {
        event.stopPropagation();
        const newStatus = !topic.isManual;

        this.profileService.setTopicManualStatus(topic.id, newStatus).subscribe({
            next: (updatedTopic) => {
                // Update the topic in the list
                const currentTopics = this.rawTopics();
                const index = currentTopics.findIndex(t => t.id === topic.id);
                if (index !== -1) {
                    const updatedTopics = [...currentTopics];
                    updatedTopics[index] = updatedTopic;
                    this.rawTopics.set(updatedTopics);
                }

                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: `Topic marked as ${newStatus ? 'Manual' : 'AI'}.`,
                });
            },
            error: (err) => {
                console.error('Failed to update topic status', err);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to update topic status.',
                });
            },
        });
    }

    private hasTourBeenShown(): boolean {
        return document.cookie.split('; ').some(cookie => cookie.startsWith(`${this.TOUR_COOKIE_NAME}=`));
    }

    private setTourCookie(): void {
        // Set cookie to expire in 1 year
        const expires = new Date();
        expires.setFullYear(expires.getFullYear() + 1);
        document.cookie = `${this.TOUR_COOKIE_NAME}=true; expires=${expires.toUTCString()}; path=/`;
    }

    private startTour(): void {
        const driverObj = driver({
            showProgress: true,
            showButtons: ['next', 'previous', 'close'],
            steps: [
                {
                    element: '.tour-first-topic',
                    popover: {
                        title: 'Focus Area',
                        description: 'Each row represents a topic identified from your activities, such as marking presentations as favorites or checking in to sessions. Topics are ranked by their weight.',
                        side: 'bottom',
                        align: 'start'
                    }
                },
                {
                    element: '.tour-topic-tag',
                    popover: {
                        title: 'AI vs Manual Topics',
                        description: 'Topics can be AI-generated or manually confirmed. Click the tag to toggle between AI and Manual. Manual topics maintain maximum weight over time, while AI topics gradually decrease in weight.',
                        side: 'bottom',
                        align: 'start'
                    }
                },
                {
                    element: '.tour-weight-bar',
                    popover: {
                        title: 'Topic Weight',
                        description: 'The weight represents how relevant this topic is to you. AI topics lose weight over a 3-year period, but manual topics maintain full weight. Higher weight topics influence your recommendations more.',
                        side: 'bottom',
                        align: 'start'
                    }
                }
            ],
            onDestroyStarted: () => {
                this.setTourCookie();
                driverObj.destroy();
            },
            onDestroyed: () => {
                window.scrollTo({ top: 0, behavior: 'smooth' });
            },
        });

        this.setTourCookie();
        driverObj.drive();
    }
}
