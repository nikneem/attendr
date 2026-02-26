import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccordionModule } from 'primeng/accordion';
import { TagModule } from 'primeng/tag';
import { AvatarModule } from 'primeng/avatar';
import { AvatarGroupModule } from 'primeng/avatargroup';
import { TooltipModule } from 'primeng/tooltip';
import { GroupDetailsStore } from '@stores/group-details.store';
import { JoinedGroupsService } from '@services/joined-groups.service';
import { CheckInDto } from '@models/check-in-dto';

import { TranslateModule } from '@ngx-translate/core';
@Component({
    selector: 'attn-member-locations',
    standalone: true,
    imports: [CommonModule, AccordionModule, TagModule, AvatarModule, AvatarGroupModule, TooltipModule,
        TranslateModule
    ],
    templateUrl: './member-locations.component.html',
    styleUrl: './member-locations.component.scss',
})
export class MemberLocationsComponent implements OnInit {
    private readonly store = inject(GroupDetailsStore);
    private readonly groupsService = inject(JoinedGroupsService);

    groupId = input.required<string>();

    group = this.store.groupDetails;
    checkIns = signal<CheckInDto[]>([]);
    loading = signal<boolean>(true);

    // Computed property to show only first 5 members per check-in
    visibleMembersPerCheckIn = 5;

    // Expand accordion when there is data
    accordionValue = computed(() => {
        return this.checkIns().length > 0 && !this.loading() ? '0' : undefined;
    });

    ngOnInit() {
        this.loadCheckIns();
    }

    private loadCheckIns() {
        this.loading.set(true);
        this.groupsService.getGroupCheckIns(this.groupId()).subscribe({
            next: checkIns => {
                this.checkIns.set(checkIns);
                this.loading.set(false);
            },
            error: error => {
                console.error('Error loading check-ins:', error);
                this.loading.set(false);
            },
        });
    }

    formatTime(dateString: string): string {
        const date = new Date(dateString);
        return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
    }

    getMemberInitials(name: string): string {
        return name
            .split(' ')
            .map(n => n[0])
            .join('')
            .toUpperCase()
            .substring(0, 2);
    }

    getVisibleMembers(checkIn: CheckInDto) {
        return checkIn.members.slice(0, this.visibleMembersPerCheckIn);
    }

    getRemainingMembersCount(checkIn: CheckInDto): number {
        return Math.max(0, checkIn.members.length - this.visibleMembersPerCheckIn);
    }

    getSpeakerNames(checkIn: CheckInDto): string {
        if (checkIn.presentationData.speakers.length === 0) {
            return 'No speakers listed';
        }
        if (checkIn.presentationData.speakers.length === 1) {
            return checkIn.presentationData.speakers[0].name;
        }
        if (checkIn.presentationData.speakers.length === 2) {
            return `${checkIn.presentationData.speakers[0].name} & ${checkIn.presentationData.speakers[1].name}`;
        }
        return `${checkIn.presentationData.speakers[0].name} & ${checkIn.presentationData.speakers.length - 1} others`;
    }
}
