import { ApplicationConfig, APP_INITIALIZER, provideBrowserGlobalErrorListeners, isDevMode } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { providePrimeNG } from 'primeng/config';
import { MessageService } from 'primeng/api';
import { provideServiceWorker } from '@angular/service-worker';

import { routes } from './app.routes';
import { authConfig } from './auth/auth.config';
import { OidcSecurityService, provideAuth, authInterceptor } from 'angular-auth-oidc-client';
import { retryInterceptor } from './shared/interceptors/retry.interceptor';
import { AuthCallbackHandler } from './auth/auth-callback.handler';
import { AttendrPreset } from './theme/attendr.preset';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor(), retryInterceptor])
    ),
    providePrimeNG({
      theme: {
        preset: AttendrPreset
      }
    }),
    MessageService,
    provideAuth(authConfig),
    // Angular Service Worker disabled - using custom service worker for push notifications
    // provideServiceWorker('ngsw-worker.js', {
    //   enabled: !isDevMode(),
    //   registrationStrategy: 'registerWhenStable:30000'
    // }),
    {
      provide: APP_INITIALIZER,
      multi: true,
      useFactory: () => {
        return async () => {
          // Unregister all existing service workers first to prevent duplicates
          if ('serviceWorker' in navigator) {
            const registrations = await navigator.serviceWorker.getRegistrations();
            for (const registration of registrations) {
              await registration.unregister();
              console.log('Unregistered service worker:', registration.scope);
            }

            // Now register only our custom service worker
            try {
              const reg = await navigator.serviceWorker.register('/service-worker.js', {
                scope: '/'
              });
              console.log('Custom service worker registered:', reg);
            } catch (err) {
              console.error('Service worker registration error:', err);
            }
          }
        };
      }
    },
    {
      provide: APP_INITIALIZER,
      multi: true,
      deps: [OidcSecurityService, AuthCallbackHandler],
      useFactory: (oidc: OidcSecurityService, authHandler: AuthCallbackHandler) => () => {
        // authHandler is injected here to ensure it's instantiated and its constructor runs
        // checkAuth will handle both regular page loads and OAuth callbacks
        return oidc.checkAuth().toPromise();
      },
    }
  ]
};
