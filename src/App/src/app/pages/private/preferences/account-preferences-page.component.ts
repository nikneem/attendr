import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ProgressBarModule } from 'primeng/progressbar';
import { MessageService } from 'primeng/api';
import { ProfileService } from '@services/profile.service';
import { ProfileStore } from '@stores/profile.store';
import { ProfileDetailsDto } from '@models/profile-details-dto';
import { UpdateProfileRequest } from '@models/update-profile-request';

@Component({
    selector: 'attn-account-preferences-page',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        CardModule,
        InputTextModule,
        ButtonModule,
        ProgressSpinnerModule,
        ProgressBarModule,
    ],
    templateUrl: './account-preferences-page.component.html',
    styleUrl: './account-preferences-page.component.scss',
})
export class AccountPreferencesPageComponent implements OnInit {
    private readonly profileService = inject(ProfileService);
    private readonly messageService = inject(MessageService);
    protected readonly profileStore = inject(ProfileStore);

    protected loading = signal<boolean>(false);
    protected saving = signal<boolean>(false);

    protected profileDetails = signal<ProfileDetailsDto | null>(null);

    // Form fields - now writable signals
    displayName = signal<string>('');
    firstName = signal<string>('');
    lastName = signal<string>('');
    email = signal<string>('');
    tagLine = signal<string>('');
    isSearchable = signal<boolean>(false);

    ngOnInit(): void {
        this.loadProfile();
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
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Profile updated successfully!',
                });
                this.saving.set(false);
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
}
