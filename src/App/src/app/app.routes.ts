import { Routes } from '@angular/router';
import { HomePageComponent } from './pages/public/home/home-page-component/home-page-component';
import { SystemInformationPageComponent } from './pages/public/system-information/system-information-page.component';
import { DisclaimerPageComponent } from './pages/public/disclaimer/disclaimer-page.component';
import { TermsOfServicePageComponent } from './pages/public/terms-of-service/terms-of-service-page.component';
import { DashboardPageComponent } from './pages/private/dashboard/dashboard-page-component/dashboard-page-component';
import { ConferencesPageComponent } from './pages/private/conferences/conferences-page.component';
import { ConferenceDetailsPageComponent } from './pages/private/conferences/conference-details-page.component';
import { ConferencePersonalSchedulePageComponent } from './pages/private/conferences/conference-personal-schedule-page.component';
import { RateSessionsPageComponent } from './pages/private/conferences/rate-sessions-page.component';
import { GroupsListPageComponent } from './pages/private/groups/groups-list-page.component';
import { GroupDetailsPageComponent } from './pages/private/groups/group-details-page.component';
import { NotificationPreferencesPageComponent } from './pages/private/preferences/notification-preferences-page.component';
import { AccountPreferencesPageComponent } from './pages/private/preferences/account-preferences-page.component';
import { FocusAreasPageComponent } from './pages/private/preferences/focus-areas-page.component';
import { PreferencesLayoutComponent } from './pages/private/preferences/preferences-layout.component';
import { TopicsListPageComponent } from './pages/private/admin/topics/topics-list-page.component';
import { ConferenceEditPageComponent } from './pages/private/conferences/conference-edit-page.component';
import { AutoLoginAllRoutesGuard } from 'angular-auth-oidc-client';
import { PrivatePageTemplateComponent } from './templates/private/private-page-template/private-page-template';

export const routes: Routes = [
    { path: '', component: HomePageComponent },
    { path: 'system-info', component: SystemInformationPageComponent },
    { path: 'disclaimer', component: DisclaimerPageComponent },
    { path: 'terms', component: TermsOfServicePageComponent },
    {
        path: 'app',
        canActivate: [AutoLoginAllRoutesGuard],
        component: PrivatePageTemplateComponent,
        children: [
            { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
            { path: 'dashboard', component: DashboardPageComponent },
            { path: 'conferences', component: ConferencesPageComponent },
            { path: 'conferences/:id', component: ConferenceDetailsPageComponent },
            { path: 'conferences/:id/schedule', component: ConferencePersonalSchedulePageComponent },
            { path: 'conferences/:id/rate', component: RateSessionsPageComponent },
            { path: 'conferences/:id/edit', component: ConferenceEditPageComponent },
            { path: 'groups', component: GroupsListPageComponent },
            { path: 'groups/:id', component: GroupDetailsPageComponent },
            {
                path: 'preferences',
                component: PreferencesLayoutComponent,
                children: [
                    { path: '', pathMatch: 'full', redirectTo: 'account' },
                    { path: 'account', component: AccountPreferencesPageComponent },
                    { path: 'topics', component: FocusAreasPageComponent },
                    { path: 'notifications', component: NotificationPreferencesPageComponent }
                ]
            },
            { path: 'topics', component: TopicsListPageComponent }
        ]
    }
];
