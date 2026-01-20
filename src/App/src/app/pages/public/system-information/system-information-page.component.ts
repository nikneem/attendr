import { Component, OnInit, OnDestroy, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, interval, takeUntil } from 'rxjs';
import { startWith, switchMap } from 'rxjs/operators';
import { SystemInformationService, ServiceInfo } from '../../../services/system-information.service';
import { environment } from '../../../../environments/environment';

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
        frontendVersion: environment.version ?? 'Unknown',
    };

    clientInfo = {
        browser: '',
        userAgent: '',
        language: '',
        timezone: '',
        screen: '',
        online: true,
    };

    githubInfo = {
        repoUrl: 'https://github.com/nikneem/attendr',
        issuesUrl: 'https://github.com/nikneem/attendr/issues',
    };

    private destroy$ = new Subject<void>();

    constructor(private systemInfoService: SystemInformationService) { }

    ngOnInit() {
        this.systemInfo = {
            platform: this.getPlatform(),
            osVersion: navigator.userAgent,
            timestamp: new Date(),
            frontendVersion: environment.version ?? 'Unknown',
        };

        this.clientInfo = {
            browser: this.getBrowserName(),
            userAgent: navigator.userAgent,
            language: navigator.language || 'Unknown',
            timezone: Intl.DateTimeFormat().resolvedOptions().timeZone || 'Unknown',
            screen: `${window.screen.width} x ${window.screen.height}`,
            online: navigator.onLine,
        };

        interval(30000)
            .pipe(
                startWith(0),
                switchMap(() => {
                    return this.systemInfoService.checkServiceHealth();
                }),
                takeUntil(this.destroy$)
            )
            .subscribe({
                next: (services) => {
                    this.services.set(services);
                    this.isLoading.set(false);
                },
                error: (error) => {
                    console.error('Error fetching service health:', error);
                    this.isLoading.set(false);
                },
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

    private getBrowserName(): string {
        const ua = navigator.userAgent;

        if (/Edg\//.test(ua)) return 'Microsoft Edge';
        if (/Chrome\//.test(ua)) return 'Chrome';
        if (/Safari\//.test(ua) && !/Chrome\//.test(ua)) return 'Safari';
        if (/Firefox\//.test(ua)) return 'Firefox';
        if (/MSIE|Trident/.test(ua)) return 'Internet Explorer';

        return 'Unknown';
    }

    private getPlatform(): string {
        const ua = navigator.userAgent;
        if (ua.indexOf('Win') !== -1) return 'Windows';
        if (ua.indexOf('Mac') !== -1) return 'macOS';
        if (ua.indexOf('Linux') !== -1) return 'Linux';
        return 'Unknown';
    }
}
