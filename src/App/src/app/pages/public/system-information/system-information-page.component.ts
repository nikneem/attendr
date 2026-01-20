import { Component, OnInit, OnDestroy, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, interval, takeUntil } from 'rxjs';
import { startWith, switchMap } from 'rxjs/operators';
import { SystemInformationService, ServiceInfo } from '../../../services/system-information.service';

@Component({
    selector: 'attn-system-information-page',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './system-information-page.component.html',
    styleUrl: './system-information-page.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemInformationPageComponent implements OnInit, OnDestroy {
    services = signal<ServiceInfo[]>([]);
    isLoading = signal(true);
    systemInfo = {
        platform: '',
        osVersion: '',
        timestamp: new Date(),
    };

    private destroy$ = new Subject<void>();

    constructor(private systemInfoService: SystemInformationService) { }

    ngOnInit() {
        // Get system info
        this.systemInfo = {
            platform: this.getPlatform(),
            osVersion: navigator.userAgent,
            timestamp: new Date(),
        };

        // Auto-refresh health checks every 30 seconds
        interval(30000)
            .pipe(
                startWith(0),
                switchMap(() => {
                    console.log('Checking service health...');
                    return this.systemInfoService.checkServiceHealth();
                }),
                takeUntil(this.destroy$)
            )
            .subscribe({
                next: (services) => {
                    console.log('Received services:', services);
                    console.log('Services array length:', services ? services.length : 'null');
                    console.log('isLoading before:', this.isLoading());
                    this.services.set(services);
                    this.isLoading.set(false);
                    console.log('isLoading after:', this.isLoading());
                    console.log('services property now:', this.services());
                },
                error: (error) => {
                    console.error('Error fetching service health:', error);
                    this.isLoading.set(false);
                },
                complete: () => {
                    console.log('Service health check completed');
                }
            });
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    refreshHealth() {
        this.isLoading.set(true);
        this.systemInfoService.checkServiceHealth().subscribe({
            next: (services) => {
                this.services.set(services);
                this.isLoading.set(false);
                this.systemInfo.timestamp = new Date();
            },
            error: (error) => {
                console.error('Error fetching service health:', error);
                this.isLoading.set(false);
            },
        });
    }

    getHealthBadgeClass(isHealthy: boolean | null): string {
        if (isHealthy === null) return 'badge-unknown';
        return isHealthy ? 'badge-healthy' : 'badge-unhealthy';
    }

    getHealthText(isHealthy: boolean | null): string {
        if (isHealthy === null) return 'Unknown';
        return isHealthy ? 'Healthy' : 'Unhealthy';
    }

    private getPlatform(): string {
        const ua = navigator.userAgent;
        if (ua.indexOf('Win') !== -1) return 'Windows';
        if (ua.indexOf('Mac') !== -1) return 'macOS';
        if (ua.indexOf('Linux') !== -1) return 'Linux';
        return 'Unknown';
    }
}
