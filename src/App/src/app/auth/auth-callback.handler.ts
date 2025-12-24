import { Injectable, inject } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { filter, switchMap, catchError, of, take } from 'rxjs';
import { ProfileService } from '@services/profile.service';

@Injectable({
    providedIn: 'root'
})
export class AuthCallbackHandler {
    private readonly oidcSecurityService = inject(OidcSecurityService);
    private readonly profileService = inject(ProfileService);

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
            switchMap(() => {
                // Get user data from the ID token
                return this.oidcSecurityService.getUserData().pipe(
                    switchMap((userData) => {
                        // Extract user info from ID token
                        const userInfo = userData as any;
                        const firstName = userInfo?.given_name || userInfo?.nickname || '';
                        const lastName = userInfo?.family_name || userInfo?.nickname || '';
                        const email = userInfo?.email || '';
                        // Only create profile if we have the required data
                        if (firstName && lastName && email) {
                            return this.profileService.createProfile({
                                firstName,
                                lastName,
                                email
                            }).pipe(
                                catchError(error => {
                                    // Silently handle errors (profile might already exist)
                                    console.warn('Profile creation failed:', error);
                                    return of(null);
                                })
                            );
                        }
                        return of(null);
                    })
                );
            })
        ).subscribe();
    }
}
