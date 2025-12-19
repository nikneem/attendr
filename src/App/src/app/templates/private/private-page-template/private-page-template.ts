import { Component, ViewChild } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MenubarModule } from 'primeng/menubar';
import { MenuModule } from 'primeng/menu';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
import { MenuItem } from 'primeng/api';
import { Menu } from 'primeng/menu';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
    selector: 'attn-private-page-template',
    imports: [MenubarModule, MenuModule, ButtonModule, AvatarModule, RouterLink, RouterOutlet],
    templateUrl: './private-page-template.html',
    styleUrl: './private-page-template.scss',
})
export class PrivatePageTemplateComponent {
    @ViewChild('accountMenu') accountMenu?: Menu;

    items: MenuItem[] = [
        { label: 'Dashboard', routerLink: ['/app/dashboard'] },
        { label: 'Groups', routerLink: ['/app/groups'] },
        { label: 'Conferences', routerLink: ['/app/conferences'] },
        { label: 'Reviews', routerLink: ['/app/reviews'] },
    ];

    accountItems: MenuItem[] = [
        { label: 'Preferences', icon: 'pi pi-sliders-h', routerLink: ['/app/preferences'] },
        { separator: true },
        { label: 'Log out', icon: 'pi pi-sign-out', command: () => this.logout() },
    ];

    constructor(private oidcSecurityService: OidcSecurityService) { }

    toggleAccountMenu(event: Event) {
        this.accountMenu?.toggle(event);
    }

    logout() {
        this.oidcSecurityService.logoff().subscribe();
    }
}
