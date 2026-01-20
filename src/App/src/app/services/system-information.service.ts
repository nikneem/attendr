import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, of } from 'rxjs';
import { map, catchError, timeout } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface ServiceInfo {
    name: string;
    version: string | null;
    isHealthy: boolean | null;
    lastChecked: Date;
}

interface VersionResponse {
    version: string;
}

@Injectable({
    providedIn: 'root',
})
export class SystemInformationService {
    private readonly backendHost = environment.apiUrl;

    private readonly serviceEndpoints = [
        { name: 'Conferences', path: '/conferences/version' },
        { name: 'Notifications', path: '/notifications/version' },
        { name: 'Groups', path: '/groups/version' },
        { name: 'Profiles', path: '/profiles/version' },
        { name: 'Presence', path: '/presence/version' },
    ];

    private readonly healthCheckTimeout = 5000; // 5 seconds

    constructor(private http: HttpClient) { }

    checkServiceHealth(): Observable<ServiceInfo[]> {
        const healthChecks = this.serviceEndpoints.map((service) =>
            this.checkSingleService(service.name, service.path)
        );

        console.log('Creating forkJoin with', healthChecks.length, 'health checks');
        return forkJoin(healthChecks).pipe(
            map((results) => {
                console.log('Health check results:', results);
                console.log('Results length:', results.length);
                console.log('Results details:', results.map(r => ({ name: r.name, version: r.version, isHealthy: r.isHealthy })));
                return results;
            })
        );
    }

    private checkSingleService(
        serviceName: string,
        endpoint: string
    ): Observable<ServiceInfo> {
        const url = `${this.backendHost}${endpoint}`;
        return this.http
            .get<VersionResponse>(url)
            .pipe(
                timeout(this.healthCheckTimeout),
                map((response) => ({
                    name: serviceName,
                    version: response.version,
                    isHealthy: true,
                    lastChecked: new Date(),
                })),
                catchError((error) => {
                    console.warn(
                        `Health check failed for ${serviceName} at ${url}:`,
                        error
                    );
                    return of({
                        name: serviceName,
                        version: null,
                        isHealthy: false,
                        lastChecked: new Date(),
                    });
                })
            );
    }
}
