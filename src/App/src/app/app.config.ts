import { ApplicationConfig, APP_INITIALIZER, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { providePrimeNG } from 'primeng/config';
import { MessageService } from 'primeng/api';

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
