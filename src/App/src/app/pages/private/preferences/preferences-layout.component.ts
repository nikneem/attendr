import { Component, computed, signal, inject } from '@angular/core';
import { Router, RouterOutlet, RouterLink, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MenuItem } from 'primeng/api';
import { filter } from 'rxjs';

@Component({
    selector: 'attn-preferences-layout',
    standalone: true,
    imports: [CommonModule, RouterOutlet, RouterLink],
    templateUrl: './preferences-layout.component.html',
    styleUrl: './preferences-layout.component.scss',
})
export class PreferencesLayoutComponent {
    private readonly router = inject(Router);

    // Track the active tab index based on the current route
    activeTab = signal<'account' | 'topics' | 'notifications'>('account');

    constructor() {
        // Update active tab when route changes
        this.router.events
            .pipe(filter(event => event instanceof NavigationEnd))
            .subscribe(() => {
                this.updateActiveTabFromRoute();
            });

        // Set initial active tab
        this.updateActiveTabFromRoute();
    }

    private updateActiveTabFromRoute(): void {
        const currentUrl = this.router.url;

        if (currentUrl.includes('/preferences/account')) {
            this.activeTab.set('account');
        } else if (currentUrl.includes('/preferences/topics')) {
            this.activeTab.set('topics');
        } else if (currentUrl.includes('/preferences/notifications')) {
            this.activeTab.set('notifications');
        }
    }

    isActive(tab: string): boolean {
        return this.activeTab() === tab;
    }
}
