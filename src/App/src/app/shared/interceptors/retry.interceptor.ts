import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { retry, timer } from 'rxjs';
import { environment } from '../../../environments/environment';

export const retryInterceptor: HttpInterceptorFn = (req, next) => {
    // Only apply retry logic to our API
    if (!req.url.startsWith(environment.apiUrl)) {
        return next(req);
    }

    return next(req).pipe(
        retry({
            count: 3,
            delay: (error: HttpErrorResponse, retryCount: number) => {
                // Don't retry on client errors (4xx) except 429 (Too Many Requests)
                if (error.status >= 400 && error.status < 500 && error.status !== 429) {
                    throw error;
                }

                // Don't retry on authentication/authorization errors
                if (error.status === 401 || error.status === 403) {
                    throw error;
                }

                // Exponential backoff: 1s, 2s, 4s
                const delayMs = Math.pow(2, retryCount - 1) * 1000;
                console.log(`Retrying request (attempt ${retryCount}/3) after ${delayMs}ms delay`);
                return timer(delayMs);
            },
        })
    );
};
