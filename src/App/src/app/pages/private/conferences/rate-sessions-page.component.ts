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

        // Load 3 cards initially
        const loadPromises = [this.fetchCard(), this.fetchCard(), this.fetchCard()];

        Promise.all(loadPromises).then((cards) => {
            const validCards = cards.filter((c) => c !== null) as CardInStack[];
            
            // If no cards loaded, show empty state card
            if (validCards.length === 0) {
                this.cards.set([{
                    presentation: null,
                    rating: null,
                    isEmpty: true,
                }]);
            } else {
                this.cards.set(validCards);
            }
            
            this.loading.set(false);
        });
    }

    private fetchCard(): Promise<CardInStack | null> {
        return new Promise((resolve) => {
            this.presenceService.getPresentationToRate(this.conferenceId()).subscribe({
                next: (presentation) => {
                    resolve({
                        presentation,
                        rating: null,
                        isEmpty: false,
                    });
                },
                error: (err) => {
                    if (err.status === 204) {
                        // No more presentations
                        resolve(null);
                    } else {
                        console.error('Error loading presentation:', err);
                        resolve(null);
                    }
                },
            });
        });
    }

    private addEmptyCard(): void {
        const currentCards = this.cards();
        if (!currentCards.some((c) => c.isEmpty)) {
            this.cards.set([
                ...currentCards,
                {
                    presentation: null,
                    rating: null,
                    isEmpty: true,
                },
            ]);
        }
    }

    updateRating(index: number, rating: number | null): void {
        const currentCards = this.cards();
        if (currentCards[index] && !currentCards[index].isEmpty) {
            currentCards[index] = { ...currentCards[index], rating };
            this.cards.set([...currentCards]);
        }
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
        const activeCard = this.activeCard();
        if (!activeCard || activeCard.isEmpty || this.submitting()) return;

        const pres = activeCard.presentation;
        if (!pres) return;

        this.submitting.set(true);
        this.isAnimating.set(true);

        // Animate card out
        this.swipeOffset.set(isFavorite ? window.innerWidth : -window.innerWidth);

        setTimeout(() => {
            this.presenceService
                .ratePresentation(this.conferenceId(), pres.presentationId, activeCard.rating, isFavorite)
                .subscribe({
                    next: () => {
                        // Remove the top card
                        const remainingCards = this.cards().slice(1);

                        // Fetch a new card for the bottom
                        this.fetchCard().then((newCard) => {
                            if (newCard) {
                                this.cards.set([...remainingCards, newCard]);
                            } else {
                                // No more cards available
                                if (remainingCards.length === 0) {
                                    // Show empty state card
                                    this.cards.set([{
                                        presentation: null,
                                        rating: null,
                                        isEmpty: true,
                                    }]);
                                } else {
                                    this.cards.set(remainingCards);
                                    // Add empty state card to the bottom
                                    this.addEmptyCard();
                                }
                            }
                            this.submitting.set(false);
                            this.swipeOffset.set(0);
                            
                            // Reset animation state after cards have repositioned
                            setTimeout(() => {
                                this.isAnimating.set(false);
                            }, 50);
                        });
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
