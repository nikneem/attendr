import { Injectable, inject, signal, computed } from '@angular/core';
import { PresenceService } from './presence.service';
import { ConferenceAttendanceDto } from '../models/conference-attendance-dto';

@Injectable({
    providedIn: 'root',
})
export class ConferenceAttendanceStore {
    private readonly presenceService = inject(PresenceService);

    private attendanceData = signal<ConferenceAttendanceDto | null>(null);
    private loading = signal(false);
    private error = signal<string | null>(null);

    // Public readonly signals
    readonly data = this.attendanceData.asReadonly();
    readonly isLoading = this.loading.asReadonly();
    readonly errorMessage = this.error.asReadonly();

    // Computed signals
    readonly isFollowing = computed(() => this.attendanceData()?.isFollowing ?? false);
    readonly isAttending = computed(() => this.attendanceData()?.isAttending ?? false);
    readonly favoritePresentationIds = computed(() => this.attendanceData()?.favoritePresentationIds ?? []);
    readonly recommendedPresentationIds = computed(() => this.attendanceData()?.recommendedPresentationIds ?? []);

    isFavorite(presentationId: string): boolean {
        return this.favoritePresentationIds().includes(presentationId);
    }

    isRecommended(presentationId: string): boolean {
        return this.recommendedPresentationIds().includes(presentationId);
    }

    loadAttendance(conferenceId: string): void {
        this.loading.set(true);
        this.error.set(null);

        this.presenceService.getConferenceAttendance(conferenceId).subscribe({
            next: (data) => {
                this.attendanceData.set(data);
                this.loading.set(false);
            },
            error: (err) => {
                console.error('Error loading conference attendance:', err);
                this.error.set('Failed to load conference attendance');
                this.loading.set(false);
            },
        });
    }

    clear(): void {
        this.attendanceData.set(null);
        this.loading.set(false);
        this.error.set(null);
    }

    // Update local state after following/unfollowing
    setFollowing(isFollowing: boolean): void {
        const current = this.attendanceData();
        if (current) {
            this.attendanceData.set({
                ...current,
                isFollowing,
            });
        }
    }

    // Update local state after changing attendance
    setAttending(isAttending: boolean): void {
        const current = this.attendanceData();
        if (current) {
            this.attendanceData.set({
                ...current,
                isAttending,
            });
        }
    }

    // Add favorite locally
    addFavorite(presentationId: string): void {
        const current = this.attendanceData();
        if (current && !current.favoritePresentationIds.includes(presentationId)) {
            this.attendanceData.set({
                ...current,
                favoritePresentationIds: [...current.favoritePresentationIds, presentationId],
            });
        }
    }

    // Remove favorite locally
    removeFavorite(presentationId: string): void {
        const current = this.attendanceData();
        if (current) {
            this.attendanceData.set({
                ...current,
                favoritePresentationIds: current.favoritePresentationIds.filter((id) => id !== presentationId),
            });
        }
    }
}
