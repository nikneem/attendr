import { Injectable, signal, computed } from '@angular/core';
import { ProfileDto } from '../models/profile-dto';

@Injectable({
    providedIn: 'root',
})
export class ProfileStore {
    private readonly _profile = signal<ProfileDto | null>(null);
    private readonly _loading = signal<boolean>(false);
    private readonly _error = signal<string | null>(null);
    private readonly _isLoaded = signal<boolean>(false);

    readonly profile = this._profile.asReadonly();
    readonly loading = this._loading.asReadonly();
    readonly error = this._error.asReadonly();
    readonly isLoaded = this._isLoaded.asReadonly();

    // Computed signals for easy access to individual fields
    readonly firstName = computed(() => this._profile()?.firstName ?? '');
    readonly lastName = computed(() => this._profile()?.lastName ?? '');
    readonly email = computed(() => this._profile()?.email ?? '');
    readonly profilePicture = computed(() => this._profile()?.profilePicture);
    readonly isAdmin = computed(() => this._profile()?.isAdmin ?? false);
    readonly displayName = computed(() => {
        const profile = this._profile();
        if (!profile) return '';
        return `${profile.firstName} ${profile.lastName}`.trim();
    });

    /**
     * Set the profile data
     */
    setProfile(profile: ProfileDto): void {
        this._profile.set(profile);
        this._error.set(null);
        this._isLoaded.set(true);
    }

    /**
     * Update profile fields partially
     */
    updateProfile(updates: Partial<ProfileDto>): void {
        const current = this._profile();
        if (current) {
            this._profile.set({ ...current, ...updates });
        }
    }

    /**
     * Set admin status
     */
    setAdmin(isAdmin: boolean): void {
        this.updateProfile({ isAdmin });
    }

    /**
     * Set loading state
     */
    setLoading(loading: boolean): void {
        this._loading.set(loading);
    }

    /**
     * Set error state
     */
    setError(error: string | null): void {
        this._error.set(error);
    }

    /**
     * Clear profile state
     */
    clear(): void {
        this._profile.set(null);
        this._loading.set(false);
        this._error.set(null);
        this._isLoaded.set(false);
    }
}
