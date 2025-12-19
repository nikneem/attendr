import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { retry, timer, catchError, throwError } from 'rxjs';
import { MessageService } from 'primeng/api';
import { environment } from '../../../environments/environment';

export const retryInterceptor: HttpInterceptorFn = (req, next) => {
    const messageService = inject(MessageService);

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
        }),
        catchError((error: HttpErrorResponse) => {
            // Check if this is a connection error (status 0 or network error)
            if (error.status === 0 || error.status === 504 || error.status === 503 || error.status === 502) {
                messageService.add({
                    severity: 'error',
                    summary: 'Server Error',
                    detail: 'Unable to connect to the server. Please check your connection and try again.',
                    life: 5000
                });
            } else if (error.status >= 500) {
                messageService.add({
                    severity: 'error',
                    summary: 'Server Error',
                    detail: 'A server error occurred. Please try again later.',
                    life: 5000
                });
            }
            return throwError(() => error);
        })
    );
};
