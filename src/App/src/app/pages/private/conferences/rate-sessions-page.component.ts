import { Component, inject, OnInit, signal, computed, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { RatingModule } from 'primeng/rating';
import { FormsModule } from '@angular/forms';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { PresenceService } from '@services/presence.service';
import { PresentationToRateDto } from '@models/presentation-to-rate-dto';

@Component({
    selector: 'attn-rate-sessions-page',
    standalone: true,
    imports: [CommonModule, CardModule, ButtonModule, RatingModule, FormsModule, ProgressSpinnerModule],
    templateUrl: './rate-sessions-page.component.html',
    styleUrl: './rate-sessions-page.component.scss',
})
export class RateSessionsPageComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly presenceService = inject(PresenceService);
    private readonly messageService = inject(MessageService);

    conferenceId = signal<string>('');
    presentation = signal<PresentationToRateDto | null>(null);
    rating = signal<number | null>(null);
    loading = signal(true);
    error = signal<string | null>(null);
    submitting = signal(false);
    noMorePresentations = signal(false);

    // Swipe/drag state
    private startX = 0;
    private startY = 0;
    private currentX = 0;
    private currentY = 0;
    private isDragging = false;
    swipeOffset = signal(0);
    swipeDirection = computed(() => {
        const offset = this.swipeOffset();
        if (Math.abs(offset) < 50) return 'none';
        return offset > 0 ? 'right' : 'left';
    });

    ngOnInit(): void {
        this.conferenceId.set(this.route.snapshot.paramMap.get('id') || '');
        if (this.conferenceId()) {
            this.loadPresentation();
        }
    }

    loadPresentation(): void {
        this.loading.set(true);
        this.error.set(null);
        this.rating.set(null);
        this.swipeOffset.set(0);

        this.presenceService.getPresentationToRate(this.conferenceId()).subscribe({
            next: (presentation) => {
                this.presentation.set(presentation);
                this.loading.set(false);
                this.noMorePresentations.set(false);
            },
            error: (err) => {
                if (err.status === 204) {
                    this.noMorePresentations.set(true);
                    this.presentation.set(null);
                } else {
                    console.error('Error loading presentation:', err);
                    this.error.set('Failed to load presentation. Please try again.');
                }
                this.loading.set(false);
            },
        });
    }

    // Mouse events
    onMouseDown(event: MouseEvent): void {
        if (this.submitting()) return;
        this.startDrag(event.clientX, event.clientY);
        event.preventDefault();
    }

    @HostListener('document:mousemove', ['$event'])
    onMouseMove(event: MouseEvent): void {
        if (this.isDragging) {
            this.updateDrag(event.clientX, event.clientY);
        }
    }

    @HostListener('document:mouseup')
    onMouseUp(): void {
        if (this.isDragging) {
            this.endDrag();
        }
    }

    // Touch events
    onTouchStart(event: TouchEvent): void {
        if (this.submitting()) return;
        const touch = event.touches[0];
        this.startDrag(touch.clientX, touch.clientY);
    }

    onTouchMove(event: TouchEvent): void {
        if (this.isDragging) {
            const touch = event.touches[0];
            this.updateDrag(touch.clientX, touch.clientY);
            event.preventDefault();
        }
    }

    onTouchEnd(): void {
        if (this.isDragging) {
            this.endDrag();
        }
    }

    private startDrag(x: number, y: number): void {
        this.isDragging = true;
        this.startX = x;
        this.startY = y;
        this.currentX = x;
        this.currentY = y;
    }

    private updateDrag(x: number, y: number): void {
        this.currentX = x;
        this.currentY = y;
        const deltaX = this.currentX - this.startX;
        const deltaY = Math.abs(this.currentY - this.startY);

        // Only apply horizontal swipe if horizontal movement dominates
        if (Math.abs(deltaX) > deltaY) {
            this.swipeOffset.set(deltaX);
        }
    }

    private endDrag(): void {
        this.isDragging = false;
        const offset = this.swipeOffset();

        // Threshold for triggering swipe action (30% of screen width)
        const threshold = window.innerWidth * 0.3;

        if (Math.abs(offset) >= threshold) {
            const isFavorite = offset > 0;
            this.submitRating(isFavorite);
        } else {
            // Reset if threshold not met
            this.swipeOffset.set(0);
        }
    }

    submitRating(isFavorite: boolean): void {
        const pres = this.presentation();
        if (!pres || this.submitting()) return;

        this.submitting.set(true);

        // Animate card out
        this.swipeOffset.set(isFavorite ? window.innerWidth : -window.innerWidth);

        setTimeout(() => {
            this.presenceService
                .ratePresentation(this.conferenceId(), pres.presentationId, this.rating(), isFavorite)
                .subscribe({
                    next: () => {
                        this.submitting.set(false);
                        this.loadPresentation();
                    },
                    error: (err) => {
                        console.error('Error rating presentation:', err);
                        this.messageService.add({
                            severity: 'error',
                            summary: 'Error',
                            detail: 'Failed to save rating. Please try again.',
                        });
                        this.submitting.set(false);
                        this.swipeOffset.set(0);
                    },
                });
        }, 300);
    }

    formatDateTime(dateTime: string): string {
        const date = new Date(dateTime);
        return date.toLocaleString('en-US', {
            weekday: 'short',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    }

    goBack(): void {
        this.router.navigate(['/app/conferences', this.conferenceId()]);
    }
}
