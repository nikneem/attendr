import { Component, OnInit, AfterViewInit, inject } from '@angular/core';
import { JoinedGroupsComponent } from '@components/joined-groups/joined-groups.component';
import { MyConferencesComponent } from '@components/my-conferences/my-conferences.component';
import { HereNowComponent } from '@components/here-now/here-now.component';
import { driver } from 'driver.js';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'attn-dashboard-page',
    imports: [JoinedGroupsComponent, MyConferencesComponent, HereNowComponent,
        TranslateModule
    ],
    templateUrl: './dashboard-page-component.html',
    styleUrl: './dashboard-page-component.scss',
})
export class DashboardPageComponent implements OnInit, AfterViewInit {
    private readonly TOUR_COOKIE_NAME = 'dashboardTourCompleted';
    private readonly translate = inject(TranslateService);

    ngOnInit(): void {
        // Component initialization
    }

    ngAfterViewInit(): void {
        // Start tour after view is initialized and only if not shown before
        if (!this.hasTourBeenShown()) {
            // Small delay to ensure all child components are rendered
            setTimeout(() => {
                this.startTour();
            }, 1000);
        }
    }

    private hasTourBeenShown(): boolean {
        return document.cookie.split('; ').some(cookie => cookie.startsWith(`${this.TOUR_COOKIE_NAME}=`));
    }

    private setTourCookie(): void {
        // Set cookie to expire in 1 year
        const expires = new Date();
        expires.setFullYear(expires.getFullYear() + 1);
        document.cookie = `${this.TOUR_COOKIE_NAME}=true; expires=${expires.toUTCString()}; path=/`;
    }

    private startTour(): void {
        const t = (key: string): string => String(this.translate.instant(key));
        const driverObj = driver({
            showProgress: true,
            showButtons: ['next', 'previous', 'close'],
            steps: [
                {
                    element: '.tour-here-now',
                    popover: {
                        title: t('TOUR.DASHBOARD.STEP1_TITLE'),
                        description: t('TOUR.DASHBOARD.STEP1_DESC'),
                        side: 'bottom',
                        align: 'start'
                    }
                },
                {
                    element: '.tour-my-conferences',
                    popover: {
                        title: t('TOUR.DASHBOARD.STEP2_TITLE'),
                        description: t('TOUR.DASHBOARD.STEP2_DESC'),
                        side: 'bottom',
                        align: 'start'
                    }
                },
                {
                    element: '.tour-joined-groups',
                    popover: {
                        title: t('TOUR.DASHBOARD.STEP3_TITLE'),
                        description: t('TOUR.DASHBOARD.STEP3_DESC'),
                        side: 'bottom',
                        align: 'start'
                    }
                }
            ],
            onDestroyStarted: () => {
                this.setTourCookie();
                driverObj.destroy();
            },
            onDestroyed: () => {
                window.scrollTo({ top: 0, behavior: 'smooth' });
            },
        });
        driverObj.drive();
    }
}
