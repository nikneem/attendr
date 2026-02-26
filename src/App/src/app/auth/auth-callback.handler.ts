import { Injectable, inject } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { filter, switchMap, catchError, of, take, delay } from 'rxjs';
import { ProfileService } from '@services/profile.service';
import { ProfileStore } from '@stores/profile.store';
import { LanguageService } from '@services/language.service';

@Injectable({
    providedIn: 'root'
})
export class AuthCallbackHandler {
    private readonly oidcSecurityService = inject(OidcSecurityService);
    private readonly profileService = inject(ProfileService);
    private readonly profileStore = inject(ProfileStore);
    private readonly languageService = inject(LanguageService);

    constructor() {
        // Listen for authentication state changes after initialization
        this.setupAuthListener();
    }

    /**
     * Listen to authentication state and create user profile on first login
     */
    private setupAuthListener() {
        // Use isAuthenticated$ observable to listen for auth state changes
        // This won't trigger additional checkAuth() calls
        this.oidcSecurityService.isAuthenticated$.pipe(
            filter(({ isAuthenticated }) => isAuthenticated),
            take(1), // Only handle the first authentication
            delay(100), // Small delay to ensure token data is fully populated
            switchMap(() => {
                // Apply authenticated language (stored > browser > fallback) and persist
                this.languageService.applyAuthenticatedLanguage();

                // Set loading state
                this.profileStore.setLoading(true);

                // Get user data from the ID token
                return this.oidcSecurityService.getUserData().pipe(
                    take(1),
                    switchMap((userData) => {
                        // Extract user info from ID token
                        const userInfo = userData as any;
                        const firstName = userInfo?.given_name || userInfo?.nickname || '';
                        const lastName = userInfo?.family_name || userInfo?.nickname || '';
                        const email = userInfo?.email || '';
                        const profilePicture = userInfo?.picture;

                        // Create profile in backend and use the server response
                        if (firstName && lastName && email) {
                            return this.profileService.createProfile({
                                firstName,
                                lastName,
                                email
                            }).pipe(
                                switchMap(profileResult => {
                                    // Populate profile store with server response
                                    this.profileStore.setProfile({
                                        firstName: profileResult.firstName,
                                        lastName: profileResult.lastName,
                                        email: profileResult.email,
                                        profilePicture, // From JWT token
                                        isAdmin: false // Will be updated by checkAdmin()
                                    });
                                    this.profileStore.setLoading(false);
                                    // Check admin status
                                    this.checkAdmin();
                                    return of(profileResult);
                                }),
                                catchError(error => {
                                    // Handle errors and set error state
                                    console.error('Profile creation failed:', error);
                                    this.profileStore.setError('Failed to load profile. Please try again.');
                                    this.profileStore.setLoading(false);
                                    return of(null);
                                })
                            );
                        }
                        this.profileStore.setLoading(false);
                        return of(null);
                    })
                );
            })
        ).subscribe();
    }

    private checkAdmin() {
        this.oidcSecurityService
            .getPayloadFromAccessToken(false)
            .subscribe((userData) => {
                if (userData.permissions) {
                    let permissions = userData.permissions as string[];
                    // Check if user has admin permissions and update store
                    const isAdmin = permissions.includes('admin:attendr');
                    if (isAdmin) {
                        this.profileStore.setAdmin(true);
                    }
                }
            });
    }
}
