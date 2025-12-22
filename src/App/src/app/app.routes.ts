import { Routes } from '@angular/router';
import { HomePageComponent } from './pages/public/home/home-page-component/home-page-component';
import { DashboardPageComponent } from './pages/private/dashboard/dashboard-page-component/dashboard-page-component';
import { ConferencesPageComponent } from './pages/private/conferences/conferences-page.component';
import { ConferenceDetailsPageComponent } from './pages/private/conferences/conference-details-page.component';
import { GroupsListPageComponent } from './pages/private/groups/groups-list-page.component';
import { AutoLoginAllRoutesGuard } from 'angular-auth-oidc-client';
import { PrivatePageTemplateComponent } from './templates/private/private-page-template/private-page-template';

export const routes: Routes = [
    { path: '', component: HomePageComponent },
    {
        path: 'app',
        canActivate: [AutoLoginAllRoutesGuard],
        component: PrivatePageTemplateComponent,
        children: [
            { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
            { path: 'dashboard', component: DashboardPageComponent },
            { path: 'conferences', component: ConferencesPageComponent },
            { path: 'conferences/:id', component: ConferenceDetailsPageComponent },
            { path: 'groups', component: GroupsListPageComponent }
        ]
    }
];
