import { Component, input, computed, signal, ElementRef, ViewChild, AfterViewInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Tabs, TabList, Tab, TabPanels, TabPanel } from 'primeng/tabs';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';
import { PresentationDto } from '../../models/presentation-dto';
import { SpeakerDto } from '../../models/speaker-dto';
import { PresentationDetailsComponent } from '../presentation-details/presentation-details.component';
import { ConferenceAttendanceStore } from '../../services/conference-attendance.store';
import { PresenceService } from '../../services/presence.service';

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

import { TranslateModule } from '@ngx-translate/core';
@Component({
    selector: 'attn-conference-schedule',
    imports: [CommonModule, Tabs, TabList, Tab, TabPanels, TabPanel, DialogModule, TooltipModule, PresentationDetailsComponent,
        TranslateModule
    ],
    templateUrl: './conference-schedule.component.html',
    styleUrl: './conference-schedule.component.scss',
})
export class ConferenceScheduleComponent implements AfterViewInit {
    private readonly attendanceStore = inject(ConferenceAttendanceStore);
    private readonly presenceService = inject(PresenceService);
    private readonly pixelsPerHour = 300;
    private readonly millisecondsPerHour = 60 * 60 * 1000;

    @ViewChild('scheduleContainer') scheduleContainer?: ElementRef<HTMLDivElement>;
    presentations = input.required<PresentationDto[]>();
    startDate = input.required<string>();
    endDate = input.required<string>();
    favoritePresentationIds = input<string[]>([]);
    recommendedPresentationIds = input<string[]>([]);
    conferenceId = input.required<string>();

    selectedPresentation = signal<PresentationDto | null>(null);
    showDialog = signal<boolean>(false);

    ngAfterViewInit(): void {
        // Convert vertical scroll to horizontal scroll with 4x speed for high sensitivity
        const container = this.scheduleContainer?.nativeElement;
        if (container) {
            container.addEventListener('wheel', (e: WheelEvent) => {
                if (e.deltaY !== 0) {
                    e.preventDefault();
                    container.scrollLeft += e.deltaY * 4;
                }
            }, { passive: false });
        }
    }

    scheduleDays = computed(() => {
        const presentations = this.presentations();
        if (!presentations || presentations.length === 0) {
            return [];
        }

        // Group presentations by day
        const dayMap = new Map<string, PresentationDto[]>();
        presentations.forEach((pres) => {
            const presDate = this.parseDateTime(pres.startDateTime);
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
            const startTimes = dayPresentations.map((p) => this.parseDateTime(p.startDateTime).getTime());
            const endTimes = dayPresentations.map((p) => this.parseDateTime(p.endDateTime).getTime());
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

        day.presentations.forEach((pres) => {
            const presStart = this.parseDateTime(pres.startDateTime);
            const presEnd = this.parseDateTime(pres.endDateTime);
            const roomIndex = day.rooms.indexOf(pres.roomName);

            // Calculate offset from day start time in milliseconds
            const offsetFromStart = presStart.getTime() - day.startTime.getTime();
            const presDuration = presEnd.getTime() - presStart.getTime();

            // Convert milliseconds to pixels (300px per hour)
            const startPixels = (offsetFromStart / this.millisecondsPerHour) * this.pixelsPerHour;
            const widthPixels = (presDuration / this.millisecondsPerHour) * this.pixelsPerHour;

            sessions.push({
                presentation: pres,
                roomIndex,
                startPixels,
                widthPixels,
            });
        });

        return sessions;
    }

    formatTime(dateInput: string | Date): string {
        const date = typeof dateInput === 'string' ? this.parseDateTime(dateInput) : dateInput;
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        return `${hours}:${minutes}`;
    }

    getTimeLabels(day: ScheduleDay): { label: string; leftPixels: number }[] {
        const labels: { label: string; leftPixels: number }[] = [];
        const start = new Date(day.startTime);
        const end = new Date(day.endTime);

        let current = new Date(start);
        let index = 0;
        while (current <= end) {
            labels.push({
                label: this.formatTime(current),
                leftPixels: index * this.pixelsPerHour,
            });
            current.setHours(current.getHours() + 1);
            index += 1;
        }

        return labels;
    }

    getTimelineWidth(day: ScheduleDay): number {
        const durationMs = Math.max(0, day.endTime.getTime() - day.startTime.getTime());
        const hours = Math.max(1, Math.round(durationMs / this.millisecondsPerHour));
        return hours * this.pixelsPerHour;
    }

    private parseDateTime(value: string): Date {
        const hasTimeZone = /([zZ]|[+-]\d{2}:?\d{2})$/.test(value);
        return new Date(hasTimeZone ? value : `${value}Z`);
    }

    getSpeakerNames(presentation: PresentationDto): string {
        return presentation.speakers.map(s => s.name).join(', ');
    }

    getSpeakerImages(presentation: PresentationDto): SpeakerDto[] {
        return presentation.speakers.filter(s => s.profilePictureUrl);
    }

    isFavorite(presentationId: string): boolean {
        return this.favoritePresentationIds().includes(presentationId);
    }

    isRecommended(presentationId: string): boolean {
        return this.recommendedPresentationIds().includes(presentationId);
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
        const presentation = this.selectedPresentation();
        if (!presentation) {
            return;
        }

        // Update backend
        this.presenceService
            .ratePresentation(this.conferenceId(), presentation.id, null, isFavorite)
            .subscribe({
                next: () => {
                    // Update local store
                    if (isFavorite) {
                        this.attendanceStore.addFavorite(presentation.id);
                    } else {
                        this.attendanceStore.removeFavorite(presentation.id);
                    }
                },
                error: (err) => {
                    console.error('Error toggling favorite:', err);
                },
            });
    }
}
