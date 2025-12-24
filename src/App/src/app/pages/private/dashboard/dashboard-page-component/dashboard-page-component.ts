import { Component } from '@angular/core';
import { JoinedGroupsComponent } from '@components/joined-groups/joined-groups.component';
import { MyConferencesComponent } from '@components/my-conferences/my-conferences.component';

@Component({
    selector: 'attn-dashboard-page',
    imports: [JoinedGroupsComponent, MyConferencesComponent],
    templateUrl: './dashboard-page-component.html',
    styleUrl: './dashboard-page-component.scss',
})
export class DashboardPageComponent {

}
