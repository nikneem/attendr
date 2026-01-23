import { Component, OnInit, OnDestroy, inject, signal, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, NavigationStart } from '@angular/router';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ProgressBarModule } from 'primeng/progressbar';
import { MessageService } from 'primeng/api';
import { ProfileService } from '@services/profile.service';
import { ProfileStore } from '@stores/profile.store';
import { ProfileDetailsDto } from '@models/profile-details-dto';
import { UpdateProfileRequest } from '@models/update-profile-request';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil, filter } from 'rxjs/operators';

@Component({
    selector: 'attn-account-preferences-page',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        CardModule,
        InputTextModule,
        ProgressSpinnerModule,
        ProgressBarModule,
    ],
    templateUrl: './account-preferences-page.component.html',
    styleUrl: './account-preferences-page.component.scss',
})
export class AccountPreferencesPageComponent implements OnInit, OnDestroy {
    private readonly profileService = inject(ProfileService);
    private readonly messageService = inject(MessageService);
    protected readonly profileStore = inject(ProfileStore);
    private readonly router = inject(Router);

    protected loading = signal<boolean>(false);
    protected saving = signal<boolean>(false);
    private isInitialLoad = true;
    private hasPendingChanges = false;

    protected profileDetails = signal<ProfileDetailsDto | null>(null);

    // Form fields - now writable signals
    displayName = signal<string>('');
    firstName = signal<string>('');
    lastName = signal<string>('');
    email = signal<string>('');
    tagLine = signal<string>('');
    isSearchable = signal<boolean>(false);

    private saveSubject = new Subject<void>();
    private destroy$ = new Subject<void>();

    constructor() {
        // Set up debounced auto-save
        this.saveSubject
            .pipe(
                debounceTime(1500), // Wait 1.5 seconds after last change
                takeUntil(this.destroy$)
            )
            .subscribe(() => {
                if (!this.isInitialLoad) {
                    this.saveProfile();
                }
            });

        // Watch for changes in form fields
        effect(() => {
            // Access all signals to track changes
            this.displayName();
            this.firstName();
            this.lastName();
            this.tagLine();
            this.isSearchable();

            // Trigger save (will be debounced)
            if (!this.isInitialLoad) {
                this.hasPendingChanges = true;
                this.saveSubject.next();
            }
        });

        // Save on navigation away if there are pending changes
        this.router.events
            .pipe(
                filter(event => event instanceof NavigationStart),
                takeUntil(this.destroy$)
            )
            .subscribe(() => {
                if (this.hasPendingChanges && !this.saving()) {
                    // Force immediate save without debounce
                    this.saveProfileImmediate();
                }
            });
    }

    ngOnInit(): void {
        this.loadProfile();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.saveSubject.complete();
    }

    private loadProfile(): void {
        this.loading.set(true);

        this.profileService.getProfileDetails().subscribe({
            next: (profile) => {
                this.profileDetails.set(profile);
                this.displayName.set(profile.displayName);
                this.firstName.set(profile.firstName || '');
                this.lastName.set(profile.lastName || '');
                this.email.set(profile.email);
                this.tagLine.set(profile.tagLine || '');
                this.isSearchable.set(profile.isSearchable);
                this.loading.set(false);

                // Mark initial load as complete after a short delay
                setTimeout(() => {
                    this.isInitialLoad = false;
                }, 100);
            },
            error: (err) => {
                console.error('Failed to load profile', err);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to load profile details. Please try again.',
                });
                this.loading.set(false);
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

    protected saveProfile(): void {
        this.saving.set(true);

        const request: UpdateProfileRequest = {
            displayName: this.displayName(),
            firstName: this.firstName(),
            lastName: this.lastName(),
            tagLine: this.tagLine() || undefined,
            isSearchable: this.isSearchable(),
        };

        this.profileService.updateProfile(request).subscribe({
            next: (result) => {
                this.saving.set(false);
                this.hasPendingChanges = false;
                this.messageService.add({
                    severity: 'success',
                    summary: 'Saved',
                    detail: 'Your preferences have been saved successfully',
                    life: 3000,
                });
                // Update the profile store with new display name
                this.profileStore.updateProfile({ firstName: this.firstName(), lastName: this.lastName() });
            },
            error: (err) => {
                console.error('Failed to update profile', err);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to update profile. Please try again.',
                });
                this.saving.set(false);
            },
        });
    }

    private saveProfileImmediate(): void {
        const request: UpdateProfileRequest = {
            displayName: this.displayName(),
            firstName: this.firstName(),
            lastName: this.lastName(),
            tagLine: this.tagLine() || undefined,
            isSearchable: this.isSearchable(),
        };

        // Use synchronous approach or fire and forget
        this.profileService.updateProfile(request).subscribe({
            next: () => {
                this.hasPendingChanges = false;
                this.profileStore.updateProfile({ firstName: this.firstName(), lastName: this.lastName() });
            },
            error: (err) => {
                console.error('Failed to save on navigation', err);
            },
        });
    }
}
