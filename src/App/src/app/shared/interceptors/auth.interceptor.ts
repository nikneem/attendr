import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { switchMap, take } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const oidcSecurityService = inject(OidcSecurityService);

    // Only add auth header for requests to our API
    if (!req.url.startsWith(environment.apiUrl)) {
        return next(req);
    }

    return oidcSecurityService.getAccessToken().pipe(
        take(1),
        switchMap((token) => {
            if (token) {
                const clonedRequest = req.clone({
                    setHeaders: {
                        Authorization: `Bearer ${token}`,
                    },
                });
                return next(clonedRequest);
            }
            return next(req);
        })
    );
};
