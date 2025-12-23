import { Component, input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Tabs, TabList, Tab, TabPanels, TabPanel } from 'primeng/tabs';
import { DialogModule } from 'primeng/dialog';
import { PresentationDto } from '../../models/presentation-dto';
import { SpeakerDto } from '../../models/speaker-dto';
import { PresentationDetailsComponent } from '../presentation-details/presentation-details.component';

interface ScheduleDay {
    date: Date;
    dateLabel: string;
    presentations: PresentationDto[];
    rooms: string[];
    startTime: Date;
    endTime: Date;
}

interface TimelineSession {
    presentation: PresentationDto;
    roomIndex: number;
    startPixels: number;
    widthPixels: number;
}

@Component({
    selector: 'attn-conference-schedule',
    imports: [CommonModule, Tabs, TabList, Tab, TabPanels, TabPanel, DialogModule, PresentationDetailsComponent],
    templateUrl: './conference-schedule.component.html',
    styleUrl: './conference-schedule.component.scss',
})
export class ConferenceScheduleComponent {
    presentations = input.required<PresentationDto[]>();
    startDate = input.required<string>();
    endDate = input.required<string>();

    selectedPresentation = signal<PresentationDto | null>(null);
    showDialog = signal<boolean>(false);

    scheduleDays = computed(() => {
        const presentations = this.presentations();
        if (!presentations || presentations.length === 0) {
            return [];
        }

        // Group presentations by day
        const dayMap = new Map<string, PresentationDto[]>();
        presentations.forEach((pres) => {
            const presDate = new Date(pres.startDateTime);
            const dateKey = presDate.toDateString();
            if (!dayMap.has(dateKey)) {
                dayMap.set(dateKey, []);
            }
            dayMap.get(dateKey)!.push(pres);
        });

        // Create schedule days
        const days: ScheduleDay[] = [];
        const sortedDates = Array.from(dayMap.keys()).sort((a, b) => new Date(a).getTime() - new Date(b).getTime());

        sortedDates.forEach((dateKey) => {
            const dayPresentations = dayMap.get(dateKey)!;
            const date = new Date(dateKey);

            // Get unique rooms for this day
            const rooms = Array.from(new Set(dayPresentations.map((p) => p.roomName))).sort();

            // Find earliest and latest times
            const startTimes = dayPresentations.map((p) => new Date(p.startDateTime).getTime());
            const endTimes = dayPresentations.map((p) => new Date(p.endDateTime).getTime());
            const earliestTime = new Date(Math.min(...startTimes));
            const latestTime = new Date(Math.max(...endTimes));

            // Round start time down to the nearest hour for timeline alignment
            const startTime = new Date(earliestTime);
            startTime.setMinutes(0, 0, 0);

            // Round end time up to the next hour for timeline alignment
            const endTime = new Date(latestTime);
            endTime.setMinutes(0, 0, 0);
            if (latestTime.getTime() > endTime.getTime()) {
                endTime.setHours(endTime.getHours() + 1);
            }

            days.push({
                date,
                dateLabel: date.toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' }),
                presentations: dayPresentations,
                rooms,
                startTime,
                endTime,
            });
        });

        return days;
    });

    getTimelineSessions(day: ScheduleDay): TimelineSession[] {
        const sessions: TimelineSession[] = [];
        const PIXELS_PER_HOUR = 300;
        const MILLISECONDS_PER_HOUR = 60 * 60 * 1000;

        day.presentations.forEach((pres) => {
            const presStart = new Date(pres.startDateTime);
            const presEnd = new Date(pres.endDateTime);
            const roomIndex = day.rooms.indexOf(pres.roomName);

            // Calculate offset from day start time in milliseconds
            const offsetFromStart = presStart.getTime() - day.startTime.getTime();
            const presDuration = presEnd.getTime() - presStart.getTime();

            // Convert milliseconds to pixels (300px per hour)
            const startPixels = (offsetFromStart / MILLISECONDS_PER_HOUR) * PIXELS_PER_HOUR;
            const widthPixels = (presDuration / MILLISECONDS_PER_HOUR) * PIXELS_PER_HOUR;

            sessions.push({
                presentation: pres,
                roomIndex,
                startPixels,
                widthPixels,
            });
        });

        return sessions;
    }

    formatTime(dateString: string): string {
        const date = new Date(dateString);
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        return `${hours}:${minutes}`;
    }

    getTimeLabels(day: ScheduleDay): string[] {
        const labels: string[] = [];
        const start = new Date(day.startTime);
        const end = new Date(day.endTime);

        let current = new Date(start);
        while (current <= end) {
            const hours = String(current.getHours()).padStart(2, '0');
            const minutes = String(current.getMinutes()).padStart(2, '0');
            labels.push(`${hours}:${minutes}`);
            current.setHours(current.getHours() + 1);
        }

        return labels;
    }

    getSpeakerNames(presentation: PresentationDto): string {
        return presentation.speakers.map(s => s.name).join(', ');
    }

    getSpeakerImages(presentation: PresentationDto): SpeakerDto[] {
        return presentation.speakers.filter(s => s.profilePictureUrl);
    }

    openPresentationDetails(presentation: PresentationDto): void {
        this.selectedPresentation.set(presentation);
        this.showDialog.set(true);
    }

    closeDialog(): void {
        this.showDialog.set(false);
        this.selectedPresentation.set(null);
    }

    onFavoriteToggled(isFavorite: boolean): void {
        // TODO: Implement favorite functionality
        console.log('Favorite toggled:', isFavorite);
    }
}
