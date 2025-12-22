import { Injectable, inject } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { filter, switchMap, catchError, of } from 'rxjs';
import { ProfileService } from '../shared/services/profile.service';

@Injectable({
    providedIn: 'root'
})
export class AuthCallbackHandler {
    private readonly oidcSecurityService = inject(OidcSecurityService);
    private readonly profileService = inject(ProfileService);

    /**
     * Handle authentication callback to create user profile
     */
    handleAuthCallback() {
        // Listen for successful authentication
        this.oidcSecurityService.checkAuth().pipe(
            filter(({ isAuthenticated }) => isAuthenticated),
            switchMap(({ userData }) => {
                // Extract user info from ID token - userData is typed as any by the library
                const userInfo = userData as any;
                const firstName = userInfo?.given_name || '';
                const lastName = userInfo?.family_name || '';
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
        ).subscribe();
    }
}
