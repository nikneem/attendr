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
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '@services/language.service';

@Component({
    selector: 'attn-private-page-template',
    imports: [MenubarModule, MenuModule, ButtonModule, AvatarModule, RouterLink, RouterOutlet, ProgressSpinnerModule, MessageModule, NotificationsButtonComponent, BugReportButtonComponent,
        TranslateModule
    ],
    templateUrl: './private-page-template.html',
    styleUrl: './private-page-template.scss',
})
export class PrivatePageTemplateComponent {
    @ViewChild('accountMenu') accountMenu?: Menu;

    protected readonly profileStore = inject(ProfileStore);
    private readonly oidcSecurityService = inject(OidcSecurityService);
    private readonly translate = inject(TranslateService);
    private readonly languageService = inject(LanguageService);

    items = computed<MenuItem[]>(() => {
        this.languageService.currentLang(); // track language changes
        const isAdmin = this.profileStore.isAdmin();
        const itemsList: MenuItem[] = [
            { label: this.translate.instant('NAV.DASHBOARD'), routerLink: ['/app/dashboard'] },
            { label: this.translate.instant('NAV.GROUPS'), routerLink: ['/app/groups'] },
            { label: this.translate.instant('NAV.CONFERENCES'), routerLink: ['/app/conferences'] },
        ];
        if (isAdmin) {
            itemsList.push({ label: this.translate.instant('NAV.TOPICS'), routerLink: ['/app/topics'] });
        }
        return itemsList;
    });

    accountItems = computed<MenuItem[]>(() => {
        this.languageService.currentLang(); // track language changes
        return [
            { label: this.translate.instant('NAV.PREFERENCES'), icon: 'pi pi-user-edit', routerLink: ['/app/preferences/account'] },
            { label: this.translate.instant('NAV.FOCUS_AREAS'), icon: 'pi pi-tags', routerLink: ['/app/preferences/topics'] },
            { label: this.translate.instant('NAV.NOTIFICATIONS'), icon: 'pi pi-bell', routerLink: ['/app/preferences/notifications'] },
            { separator: true },
            { label: this.translate.instant('NAV.LOG_OUT'), icon: 'pi pi-sign-out', command: () => this.logout() },
        ];
    });

    toggleAccountMenu(event: Event) {
        this.accountMenu?.toggle(event);
    }

    logout() {
        this.oidcSecurityService.logoff().subscribe();
    }
}

