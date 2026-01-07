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

interface CardInStack {
    presentation: PresentationToRateDto | null;
    rating: number | null;
    isEmpty: boolean;
    isError: boolean;
    errorMessage?: string;
}

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
    cards = signal<CardInStack[]>([]);
    loading = signal(true);
    error = signal<string | null>(null);
    submitting = signal(false);
    isAnimating = signal(false);
    fetchingNewCard = signal(false);

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

    activeCard = computed(() => this.cards()[0] || null);

    ngOnInit(): void {
        this.conferenceId.set(this.route.snapshot.paramMap.get('id') || '');
        if (this.conferenceId()) {
            this.loadInitialCards();
        }
    }

    loadInitialCards(): void {
        this.loading.set(true);
        this.error.set(null);

        // Load 3 cards initially - make sequential requests to server
        this.fetchCardWithHandling().then((card1) => {
            const cards = [card1];

            // If first card is error or empty, still try to load more
            this.fetchCardWithHandling().then((card2) => {
                cards.push(card2);

                this.fetchCardWithHandling().then((card3) => {
                    cards.push(card3);

                    // Check if we have at least one valid card or if we should show error/empty
                    const hasValidCard = cards.some(c => c.presentation !== null);
                    const hasError = cards.some(c => c.isError);
                    const allEmpty = cards.every(c => c.isEmpty);

                    if (!hasValidCard && (hasError || allEmpty)) {
                        // Show only the first error or empty card as the top card
                        const topCard = cards.find(c => c.isError) || cards.find(c => c.isEmpty)!;
                        this.cards.set([topCard]);
                    } else {
                        this.cards.set(cards);
                    }

                    this.loading.set(false);
                });
            });
        });
    }

    private fetchCardWithHandling(): Promise<CardInStack> {
        return new Promise((resolve) => {
            this.presenceService.getPresentationToRate(this.conferenceId()).subscribe({
                next: (presentation) => {
                    resolve({
                        presentation,
                        rating: null,
                        isEmpty: false,
                        isError: false,
                    });
                },
                error: (err) => {
                    if (err.status === 204) {
                        // No more presentations - return empty state card
                        resolve({
                            presentation: null,
                            rating: null,
                            isEmpty: true,
                            isError: false,
                        });
                    } else {
                        // Error occurred - return error card
                        console.error('Error loading presentation:', err);
                        resolve({
                            presentation: null,
                            rating: null,
                            isEmpty: false,
                            isError: true,
                            errorMessage: err.error?.message || 'Failed to load presentation',
                        });
                    }
                },
            });
        });
    }

    private addCardToBottom(card: CardInStack): void {
        const currentCards = this.cards();
        this.cards.set([...currentCards, card]);
    }

    updateRating(index: number, rating: number | null): void {
        const currentCards = this.cards();
        if (currentCards[index] && !currentCards[index].isEmpty && !currentCards[index].isError) {
            currentCards[index] = { ...currentCards[index], rating };
            this.cards.set([...currentCards]);
        }
    }

    // Mouse events
    onMouseDown(event: MouseEvent): void {
        const activeCard = this.activeCard();
        if (this.submitting() || !activeCard || activeCard.isEmpty || activeCard.isError) return;
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
        const activeCard = this.activeCard();
        if (this.submitting() || !activeCard || activeCard.isEmpty || activeCard.isError) return;
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
        const activeCard = this.activeCard();
        if (!activeCard || activeCard.isEmpty || activeCard.isError || this.submitting()) return;

        const pres = activeCard.presentation;
        if (!pres) return;

        this.submitting.set(true);
        this.isAnimating.set(true);

        // Animate card out
        this.swipeOffset.set(isFavorite ? window.innerWidth * 1.5 : -window.innerWidth * 1.5);

        setTimeout(() => {
            this.presenceService
                .ratePresentation(this.conferenceId(), pres.presentationId, activeCard.rating, isFavorite)
                .subscribe({
                    next: () => {
                        // Remove the top card
                        const remainingCards = this.cards().slice(1);

                        // Update card positions immediately
                        this.cards.set(remainingCards);
                        this.swipeOffset.set(0);

                        // Small delay before starting to fetch new card
                        setTimeout(() => {
                            this.isAnimating.set(false);
                            this.fetchingNewCard.set(true);

                            // Fetch a new card for the bottom
                            this.fetchCardWithHandling().then((newCard) => {
                                this.addCardToBottom(newCard);
                                this.fetchingNewCard.set(false);
                            });

                            this.submitting.set(false);
                        }, 300);
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
                        this.isAnimating.set(false);
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
