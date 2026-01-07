import { Component, computed, inject, input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TimelineModule } from 'primeng/timeline';
import { TagModule } from 'primeng/tag';
import { GroupDetailsStore } from '@stores/group-details.store';

interface GroupActivity {
    id: string;
    type: 'member_joined' | 'check_in' | 'conference_followed' | 'member_left';
    timestamp: Date;
    memberName: string;
    memberProfilePicture?: string;
    details?: string; // e.g., presentation name, conference name
}

@Component({
    selector: 'attn-group-activities',
    standalone: true,
    imports: [CommonModule, CardModule, TimelineModule, TagModule],
    templateUrl: './group-activities.component.html',
    styleUrl: './group-activities.component.scss',
})
export class GroupActivitiesComponent implements OnInit {
    private readonly store = inject(GroupDetailsStore);

    groupId = input.required<string>();

    group = this.store.groupDetails;

    // Mock activities for now - in production, this would come from an API/store
    activities = computed<GroupActivity[]>(() => {
        // This is mock data - replace with actual API call
        return [
            {
                id: '1',
                type: 'member_joined',
                timestamp: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
                memberName: 'John Doe',
                memberProfilePicture: undefined,
            },
            {
                id: '2',
                type: 'check_in',
                timestamp: new Date(Date.now() - 1000 * 60 * 15), // 15 minutes ago
                memberName: 'Jane Smith',
                memberProfilePicture: undefined,
                details: 'Building Scalable Microservices',
            },
            {
                id: '3',
                type: 'conference_followed',
                timestamp: new Date(Date.now() - 1000 * 60 * 5), // 5 minutes ago
                memberName: 'Mike Johnson',
                memberProfilePicture: undefined,
                details: 'TechConf 2026',
            },
        ];
    });

    ngOnInit() {
        // TODO: Load activities from API
    }

    getActivityIcon(type: string): string {
        switch (type) {
            case 'member_joined': return 'pi pi-user-plus';
            case 'check_in': return 'pi pi-map-marker';
            case 'conference_followed': return 'pi pi-calendar-plus';
            case 'member_left': return 'pi pi-user-minus';
            default: return 'pi pi-info-circle';
        }
    }

    getActivityColor(type: string): string {
        switch (type) {
            case 'member_joined': return '#10b981';
            case 'check_in': return '#4A90E2';
            case 'conference_followed': return '#06b6d4';
            case 'member_left': return '#888';
            default: return '#888';
        }
    }

    getActivityMessage(activity: GroupActivity): string {
        switch (activity.type) {
            case 'member_joined':
                return `${activity.memberName} joined the group`;
            case 'check_in':
                return `${activity.memberName} checked in at ${activity.details}`;
            case 'conference_followed':
                return `${activity.memberName} followed ${activity.details}`;
            case 'member_left':
                return `${activity.memberName} left the group`;
            default:
                return 'Unknown activity';
        }
    }

    getRelativeTime(timestamp: Date): string {
        const now = new Date();
        const diff = now.getTime() - timestamp.getTime();
        const minutes = Math.floor(diff / (1000 * 60));
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const days = Math.floor(diff / (1000 * 60 * 60 * 24));

        if (minutes < 1) return 'Just now';
        if (minutes < 60) return `${minutes}m ago`;
        if (hours < 24) return `${hours}h ago`;
        if (days < 7) return `${days}d ago`;
        return timestamp.toLocaleDateString();
    }
}
