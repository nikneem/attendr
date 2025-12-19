import { ApplicationConfig, APP_INITIALIZER, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';
import { authConfig } from './auth/auth.config';
import { OidcSecurityService, provideAuth, authInterceptor } from 'angular-auth-oidc-client';
import { retryInterceptor } from './shared/interceptors/retry.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor(), retryInterceptor])
    ),
    providePrimeNG({
      theme: {
        preset: Aura
      }
    }),
    provideAuth(authConfig),
    {
      provide: APP_INITIALIZER,
      multi: true,
      deps: [OidcSecurityService],
      useFactory: (oidc: OidcSecurityService) => () => oidc.checkAuth().subscribe(),
    }
  ]
};
