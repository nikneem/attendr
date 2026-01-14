import { Component, computed, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccordionModule } from 'primeng/accordion';
import { TimelineModule } from 'primeng/timeline';
import { TagModule } from 'primeng/tag';
import { GroupDetailsStore } from '@stores/group-details.store';
import { GroupActivityDto } from '@models/group-activity-dto';

interface EnrichedGroupActivity extends GroupActivityDto {
    memberName: string;
}

@Component({
    selector: 'attn-group-activities',
    standalone: true,
    imports: [CommonModule, AccordionModule, TimelineModule, TagModule],
    templateUrl: './group-activities.component.html',
    styleUrl: './group-activities.component.scss',
})
export class GroupActivitiesComponent {
    private readonly store = inject(GroupDetailsStore);

    groupId = input.required<string>();

    group = this.store.groupDetails;

    activities = computed<EnrichedGroupActivity[]>(() => {
        const groupData = this.group();
        if (!groupData) return [];

        const activities = groupData.activities || [];
        const members = groupData.members || [];

        // Create a map of profileId to member name for quick lookup
        const memberMap = new Map(members.map(m => [m.id, m.name]));

        // Enrich activities with member names
        return activities.map(activity => ({
            ...activity,
            memberName: memberMap.get(activity.profileId) || 'Unknown Member'
        }));
    });

    // Expand accordion when there is data
    accordionValue = computed(() => {
        return this.activities().length > 0 ? '0' : undefined;
    });

    getActivityIcon(activityTypeId: number): string {
        switch (activityTypeId) {
            case 1: return 'pi pi-user-plus'; // ProfileJoinedGroup
            case 2: return 'pi pi-user-minus'; // ProfileLeftGroup
            case 3: return 'pi pi-map-marker'; // ProfilePresentationCheckedIn
            case 4: return 'pi pi-map-marker'; // ProfilePresentationCheckedOut
            case 5: return 'pi pi-calendar-plus'; // ProfileAttendingConference
            case 6: return 'pi pi-calendar-minus'; // ProfileLeavingConference
            default: return 'pi pi-info-circle';
        }
    }

    getActivityColor(activitySeverity: number): string {
        switch (activitySeverity) {
            case 0: return '#888'; // Low
            case 1: return '#10b981'; // Medium (green)
            case 2: return '#4A90E2'; // High (blue)
            default: return '#888';
        }
    }

    getActivityMessage(activity: EnrichedGroupActivity): string {
        return activity.description;
    }

    getMemberName(activity: EnrichedGroupActivity): string {
        return activity.memberName;
    }

    getRelativeTime(timestamp: string): string {
        const now = new Date();
        const date = new Date(timestamp);
        const diff = now.getTime() - date.getTime();
        const minutes = Math.floor(diff / (1000 * 60));
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const days = Math.floor(diff / (1000 * 60 * 60 * 24));

        if (minutes < 1) return 'Just now';
        if (minutes < 60) return `${minutes}m ago`;
        if (hours < 24) return `${hours}h ago`;
        if (days < 7) return `${days}d ago`;
        return date.toLocaleDateString();
    }
}
