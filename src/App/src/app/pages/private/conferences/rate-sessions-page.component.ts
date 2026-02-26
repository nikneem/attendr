import { Component, inject, OnInit, OnDestroy, signal, computed, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { RatingModule } from 'primeng/rating';
import { FormsModule } from '@angular/forms';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { MessageService, ConfirmationService } from 'primeng/api';
import { PresenceService } from '@services/presence.service';
import { PresentationToRateDto } from '@models/presentation-to-rate-dto';

interface CardInStack {
    presentation: PresentationToRateDto | null;
    rating: number | null;
    isEmpty: boolean;
    isError: boolean;
    errorMessage?: string;
}

import { TranslateModule } from '@ngx-translate/core';
@Component({
    selector: 'attn-rate-sessions-page',
    standalone: true,
    imports: [CommonModule, CardModule, ButtonModule, RatingModule, FormsModule, ProgressSpinnerModule, ConfirmDialogModule, TagModule,
        TranslateModule
    ],
    templateUrl: './rate-sessions-page.component.html',
    styleUrl: './rate-sessions-page.component.scss',
    providers: [ConfirmationService],
})
export class RateSessionsPageComponent implements OnInit, OnDestroy {
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly presenceService = inject(PresenceService);
    private readonly messageService = inject(MessageService);
    private readonly confirmationService = inject(ConfirmationService);

    conferenceId = signal<string>('');
    cards = signal<CardInStack[]>([]);
    loading = signal(true);
    error = signal<string | null>(null);
    submitting = signal(false);
    isAnimating = signal(false);
    fetchingNewCard = signal(false);
    showTutorialAnimation = signal(false);
    tutorialAnimationOffset = signal(0);
    private tutorialAnimationInterval?: number;

    // Swipe/drag state
    private startX = 0;
    private startY = 0;
    private currentX = 0;
    private currentY = 0;
    private isDragging = false;
    private lastMoveTime = 0;
    private lastMoveX = 0;
    private velocityX = 0;
    private gestureDirection: 'horizontal' | 'vertical' | 'unknown' = 'unknown';
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

        // Load 3 cards initially with indices 0, 1, 2 to prevent duplicates
        this.fetchCardWithHandling(0).then((card1) => {
            const cards = [card1];

            // If first card is error or empty, still try to load more
            this.fetchCardWithHandling(1).then((card2) => {
                cards.push(card2);

                this.fetchCardWithHandling(2).then((card3) => {
                    cards.push(card3);

                    // Check if we have at least one valid card or if we should show error/empty
                    const hasValidCard = cards.some(c => c.presentation !== null);
                    const hasError = cards.some(c => c.isError);
                    const allEmpty = cards.every(c => c.isEmpty);

                    console.log('Initial cards loaded:', {
                        total: cards.length,
                        hasValidCard,
                        hasError,
                        allEmpty,
                        cards: cards.map(c => ({ isEmpty: c.isEmpty, isError: c.isError, hasPresentation: !!c.presentation }))
                    });

                    if (!hasValidCard && (hasError || allEmpty)) {
                        // Show only the first error or empty card as the top card
                        const topCard = cards.find(c => c.isError) || cards.find(c => c.isEmpty)!;
                        this.cards.set([topCard]);
                    } else {
                        this.cards.set(cards);
                    }

                    this.loading.set(false);

                    // Start tutorial animation if this is the first time and we have valid cards
                    if (hasValidCard && !this.hasSeenTutorial()) {
                        this.startTutorialAnimation();
                    }
                });
            });
        });
    }

    private fetchCardWithHandling(index: number): Promise<CardInStack> {
        return new Promise((resolve) => {
            this.presenceService.getPresentationToRate(this.conferenceId(), index).subscribe({
                next: (presentation) => {
                    // Check if presentation is null or undefined (empty response)
                    if (!presentation) {
                        resolve({
                            presentation: null,
                            rating: null,
                            isEmpty: true,
                            isError: false,
                        });
                    } else {
                        resolve({
                            presentation,
                            rating: null,
                            isEmpty: false,
                            isError: false,
                        });
                    }
                },
                error: (err) => {
                    if (err.status === 404) {
                        // No presentation found at this index - return empty state card
                        resolve({
                            presentation: null,
                            rating: null,
                            isEmpty: true,
                            isError: false,
                        });
                    } else {
                        // Error occurred - return error card
                        console.error(`Error loading presentation at index ${index}:`, err);
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

        // If we already have 3 cards, replace the bottom one (index 2)
        if (currentCards.length >= 3) {
            const newCards = [...currentCards];
            newCards[2] = card;
            this.cards.set(newCards);
        } else {
            // Otherwise, just add to the bottom
            this.cards.set([...currentCards, card]);
        }
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

            // Only prevent default for horizontal swipes to allow vertical scrolling
            if (this.gestureDirection === 'horizontal') {
                event.preventDefault();
            } else if (this.gestureDirection === 'vertical') {
                // Cancel dragging if user is scrolling vertically
                this.isDragging = false;
                this.swipeOffset.set(0);
            }
        }
    }

    onTouchEnd(): void {
        if (this.isDragging) {
            this.endDrag();
        }
    }

    private startDrag(x: number, y: number): void {
        this.stopTutorialAnimation();
        this.isDragging = true;
        this.startX = x;
        this.startY = y;
        this.currentX = x;
        this.currentY = y;
        this.lastMoveX = x;
        this.lastMoveTime = Date.now();
        this.velocityX = 0;
        this.gestureDirection = 'unknown';
    }

    private updateDrag(x: number, y: number): void {
        const now = Date.now();
        const timeDelta = now - this.lastMoveTime;

        this.currentX = x;
        this.currentY = y;
        const deltaX = this.currentX - this.startX;
        const deltaY = this.currentY - this.startY;

        // Determine swipe direction if still unknown
        if (this.gestureDirection === 'unknown' && (Math.abs(deltaX) > 10 || Math.abs(deltaY) > 10)) {
            this.gestureDirection = Math.abs(deltaX) > Math.abs(deltaY) ? 'horizontal' : 'vertical';
        }

        // Calculate velocity for horizontal movement
        if (timeDelta > 0) {
            this.velocityX = (x - this.lastMoveX) / timeDelta;
        }
        this.lastMoveX = x;
        this.lastMoveTime = now;

        // Only apply horizontal swipe if horizontal movement dominates
        if (this.gestureDirection === 'horizontal') {
            this.swipeOffset.set(deltaX);
        }
    }

    private endDrag(): void {
        this.isDragging = false;
        const offset = this.swipeOffset();

        // Threshold for triggering swipe action (25% of screen width)
        const distanceThreshold = window.innerWidth * 0.25;

        // Velocity threshold for "throwing" the card (pixels per millisecond)
        const velocityThreshold = 0.5;
        const hasHighVelocity = Math.abs(this.velocityX) > velocityThreshold;

        // Trigger swipe if either distance threshold is met OR card is thrown with velocity
        if (Math.abs(offset) >= distanceThreshold || (hasHighVelocity && Math.abs(offset) > 30)) {
            const isFavorite = offset > 0;
            this.submitRating(isFavorite);
        } else {
            // Reset if thresholds not met
            this.swipeOffset.set(0);
        }

        // Reset gesture direction
        this.gestureDirection = 'unknown';
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

                            // Fetch a new card for the bottom position (index 2)
                            this.fetchCardWithHandling(2).then((newCard) => {
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

    confirmResetRatings(): void {
        this.confirmationService.confirm({
            message: 'Are you sure you wish to remove all favorited sessions and rate all sessions again?',
            header: 'Reset Ratings',
            icon: 'pi pi-exclamation-triangle',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.resetRatings();
            },
        });
    }

    private resetRatings(): void {
        this.loading.set(true);
        this.presenceService.resetConferenceRatings(this.conferenceId()).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'All ratings have been reset',
                });
                // Restart the rating process by loading initial cards
                this.loadInitialCards();
            },
            error: (err) => {
                console.error('Error resetting ratings:', err);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to reset ratings. Please try again.',
                });
                this.loading.set(false);
            },
        });
    }

    private hasSeenTutorial(): boolean {
        return document.cookie.split('; ').some(cookie => cookie.startsWith('ratingTutorialSeen='));
    }

    private setTutorialSeen(): void {
        // Set cookie to expire in 1 year
        const expires = new Date();
        expires.setFullYear(expires.getFullYear() + 1);
        document.cookie = `ratingTutorialSeen=true; expires=${expires.toUTCString()}; path=/; SameSite=Strict`;
    }

    private startTutorialAnimation(): void {
        this.showTutorialAnimation.set(true);
        let direction = 1; // 1 for right, -1 for left
        let step = 0;
        let cycleCount = 0; // Track completed cycles (left + right = 1 cycle)
        const maxOffset = 20; // Maximum offset in pixels
        const steps = 40; // Number of steps for smooth animation
        const pauseSteps = 120; // 3 seconds pause (3000ms / 25ms per step)
        let pauseCounter = 0;
        let isPaused = false;

        this.tutorialAnimationInterval = window.setInterval(() => {
            if (isPaused) {
                pauseCounter++;
                if (pauseCounter >= pauseSteps) {
                    // Resume animation
                    isPaused = false;
                    pauseCounter = 0;
                    step = 0;
                    direction = 1;
                }
                return;
            }

            step++;
            if (step >= steps) {
                direction *= -1; // Reverse direction
                step = 0;

                // Count cycles (when returning from left back to center)
                if (direction === 1) {
                    cycleCount++;
                    if (cycleCount === 1) {
                        // Completed one full cycle (right then left), pause
                        isPaused = true;
                        cycleCount = 0;
                        this.tutorialAnimationOffset.set(0);
                        return;
                    }
                }
            }

            // Ease in-out animation
            const progress = step / steps;
            const eased = progress < 0.5
                ? 2 * progress * progress
                : 1 - Math.pow(-2 * progress + 2, 2) / 2;

            this.tutorialAnimationOffset.set(direction * maxOffset * eased);
        }, 25); // Update every 25ms for smooth animation
    }

    private stopTutorialAnimation(): void {
        if (this.tutorialAnimationInterval) {
            window.clearInterval(this.tutorialAnimationInterval);
            this.tutorialAnimationInterval = undefined;
            this.showTutorialAnimation.set(false);
            this.tutorialAnimationOffset.set(0);
            this.setTutorialSeen();
        }
    }

    ngOnDestroy(): void {
        this.stopTutorialAnimation();
    }

    goBack(): void {
        this.router.navigate(['/app/conferences', this.conferenceId()]);
    }
}
