import { Component, computed, inject, input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { AvatarModule } from 'primeng/avatar';
import { AvatarGroupModule } from 'primeng/avatargroup';
import { TooltipModule } from 'primeng/tooltip';
import { GroupDetailsStore } from '@stores/group-details.store';

interface PresentationLocation {
    presentationId: string;
    presentationTitle: string;
    room: string;
    startTime: Date;
    endTime: Date;
    status: 'ongoing' | 'starting_soon'; // ongoing = currently running, starting_soon = starts within 30 minutes
    checkedInMembers: CheckedInMember[];
}

interface CheckedInMember {
    id: string;
    name: string;
    profilePictureUrl?: string;
}

@Component({
    selector: 'attn-member-locations',
    standalone: true,
    imports: [CommonModule, CardModule, TagModule, AvatarModule, AvatarGroupModule, TooltipModule],
    templateUrl: './member-locations.component.html',
    styleUrl: './member-locations.component.scss',
})
export class MemberLocationsComponent implements OnInit {
    private readonly store = inject(GroupDetailsStore);

    groupId = input.required<string>();
    
    group = this.store.groupDetails;

    // Mock locations for now - in production, this would come from an API/store
    locations = computed<PresentationLocation[]>(() => {
        const now = new Date();
        const soon = new Date(now.getTime() + 30 * 60 * 1000); // 30 minutes from now

        // This is mock data - replace with actual API call
        return [
            {
                presentationId: '1',
                presentationTitle: 'Building Scalable Microservices',
                room: 'Main Hall A',
                startTime: new Date(now.getTime() - 15 * 60 * 1000), // started 15 minutes ago
                endTime: new Date(now.getTime() + 45 * 60 * 1000), // ends in 45 minutes
                status: 'ongoing',
                checkedInMembers: [
                    { id: '1', name: 'Jane Smith', profilePictureUrl: undefined },
                    { id: '2', name: 'Mike Johnson', profilePictureUrl: undefined },
                    { id: '3', name: 'Sarah Williams', profilePictureUrl: undefined },
                ],
            },
            {
                presentationId: '2',
                presentationTitle: 'Advanced TypeScript Patterns',
                room: 'Workshop Room 2',
                startTime: new Date(now.getTime() + 10 * 60 * 1000), // starts in 10 minutes
                endTime: new Date(now.getTime() + 70 * 60 * 1000),
                status: 'starting_soon',
                checkedInMembers: [
                    { id: '4', name: 'John Doe', profilePictureUrl: undefined },
                ],
            },
        ];
    });

    ngOnInit() {
        // TODO: Load presentation locations from API
        // Consider setting up a periodic refresh to keep the data current
    }

    formatTime(date: Date): string {
        return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
    }

    getStatusColor(status: string): 'success' | 'info' {
        return status === 'ongoing' ? 'success' : 'info';
    }

    getStatusLabel(status: string): string {
        return status === 'ongoing' ? 'Live Now' : 'Starting Soon';
    }

    getStatusIcon(status: string): string {
        return status === 'ongoing' ? 'pi pi-play-circle' : 'pi pi-clock';
    }

    getMemberInitials(name: string): string {
        return name
            .split(' ')
            .map(n => n[0])
            .join('')
            .toUpperCase()
            .substring(0, 2);
    }
}
