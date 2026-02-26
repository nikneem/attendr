import { Component, OnInit, AfterViewInit } from '@angular/core';
import { JoinedGroupsComponent } from '@components/joined-groups/joined-groups.component';
import { MyConferencesComponent } from '@components/my-conferences/my-conferences.component';
import { HereNowComponent } from '@components/here-now/here-now.component';
import { driver } from 'driver.js';

import { TranslateModule } from '@ngx-translate/core';
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
        const driverObj = driver({
            showProgress: true,
            showButtons: ['next', 'previous', 'close'],
            steps: [
                {
                    element: '.tour-here-now',
                    popover: {
                        title: 'Here & Now - Active Conference',
                        description: 'When a conference is currently running, this section appears showing real-time information. It displays group member activities, your upcoming selected sessions, and allows you to check in to sessions you\'re attending.',
                        side: 'bottom',
                        align: 'start'
                    }
                },
                {
                    element: '.tour-my-conferences',
                    popover: {
                        title: 'My Conferences',
                        description: 'These are conferences you\'re following. You can follow conferences manually, and any conferences followed by groups you\'re part of are automatically added to your list.',
                        side: 'bottom',
                        align: 'start'
                    }
                },
                {
                    element: '.tour-joined-groups',
                    popover: {
                        title: 'My Groups',
                        description: 'Join or create groups to connect with other attendees. Group members can view each other\'s activities, making it easy to coordinate and share experiences at conferences.',
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
