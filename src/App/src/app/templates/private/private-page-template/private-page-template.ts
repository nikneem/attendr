import { Component, ViewChild, inject, computed } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MenubarModule } from 'primeng/menubar';
import { MenuModule } from 'primeng/menu';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
import { MenuItem } from 'primeng/api';
import { Menu } from 'primeng/menu';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { ProfileStore } from '@stores/profile.store';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { NotificationsButtonComponent } from '@components/notifications-button/notifications-button.component';
import { BugReportButtonComponent } from '@components/bug-report-button/bug-report-button.component';

@Component({
    selector: 'attn-private-page-template',
    imports: [MenubarModule, MenuModule, ButtonModule, AvatarModule, RouterLink, RouterOutlet, ProgressSpinnerModule, MessageModule, NotificationsButtonComponent, BugReportButtonComponent],
    templateUrl: './private-page-template.html',
    styleUrl: './private-page-template.scss',
})
export class PrivatePageTemplateComponent {
    @ViewChild('accountMenu') accountMenu?: Menu;

    protected readonly profileStore = inject(ProfileStore);
    private readonly oidcSecurityService = inject(OidcSecurityService);

    private readonly baseItems: MenuItem[] = [
        { label: 'Dashboard', routerLink: ['/app/dashboard'] },
        { label: 'Groups', routerLink: ['/app/groups'] },
        { label: 'Conferences', routerLink: ['/app/conferences'] },
    ];

    // Computed signal that includes Topics menu item only for admins
    items = computed<MenuItem[]>(() => {
        const isAdmin = this.profileStore.isAdmin();
        const itemsList = [...this.baseItems];
        if (isAdmin) {
            itemsList.push({ label: 'Topics', routerLink: ['/app/topics'] });
        }
        return itemsList;
    });

    accountItems: MenuItem[] = [
        { label: 'Preferences', icon: 'pi pi-user-edit', routerLink: ['/app/preferences/account'] },
        { label: 'Notifications', icon: 'pi pi-bell', routerLink: ['/app/preferences/notifications'] },
        { separator: true },
        { label: 'Log out', icon: 'pi pi-sign-out', command: () => this.logout() },
    ];

    toggleAccountMenu(event: Event) {
        this.accountMenu?.toggle(event);
    }

    logout() {
        this.oidcSecurityService.logoff().subscribe();
    }
}

