import { ApplicationConfig, APP_INITIALIZER, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';
import { authConfig } from './auth/auth.config';
import { OidcSecurityService, provideAuth } from 'angular-auth-oidc-client';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
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
