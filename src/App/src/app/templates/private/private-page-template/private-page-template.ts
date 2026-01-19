import { Component, ViewChild, inject } from '@angular/core';
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

@Component({
    selector: 'attn-private-page-template',
    imports: [MenubarModule, MenuModule, ButtonModule, AvatarModule, RouterLink, RouterOutlet, ProgressSpinnerModule, MessageModule, NotificationsButtonComponent],
    templateUrl: './private-page-template.html',
    styleUrl: './private-page-template.scss',
})
export class PrivatePageTemplateComponent {
    @ViewChild('accountMenu') accountMenu?: Menu;

    protected readonly profileStore = inject(ProfileStore);
    private readonly oidcSecurityService = inject(OidcSecurityService);

    items: MenuItem[] = [
        { label: 'Dashboard', routerLink: ['/app/dashboard'] },
        { label: 'Groups', routerLink: ['/app/groups'] },
        { label: 'Conferences', routerLink: ['/app/conferences'] },
    ];

    accountItems: MenuItem[] = [
        { label: 'Preferences', icon: 'pi pi-sliders-h', routerLink: ['/app/preferences/notifications'] },
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
