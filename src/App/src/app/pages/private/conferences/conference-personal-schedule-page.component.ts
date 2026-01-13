import { Component, inject, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Tabs, TabList, Tab, TabPanels, TabPanel } from 'primeng/tabs';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CardModule } from 'primeng/card';
import { PresenceService } from '@services/presence.service';
import { ConferenceScheduleDto, PresentationScheduleDto } from '@models/conference-schedule-dto';

interface ScheduleDay {
    date: Date;
    dateLabel: string;
    presentations: PresentationScheduleDto[];
    hours: Date[];
    startTime: Date;
    endTime: Date;
}

interface TimeSlot {
    hour: Date;
    presentations: PresentationInfo[];
}

interface PresentationInfo {
    presentation: PresentationScheduleDto;
    isFavorite: boolean;
    isPreferred: boolean;
    startPixels: number;
    heightPixels: number;
    opacity: number;
}

@Component({
    selector: 'attn-conference-personal-schedule-page',
    imports: [CommonModule, Tabs, TabList, Tab, TabPanels, TabPanel, ProgressSpinnerModule, CardModule],
    templateUrl: './conference-personal-schedule-page.component.html',
    styleUrl: './conference-personal-schedule-page.component.scss',
})
export class ConferencePersonalSchedulePageComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly presenceService = inject(PresenceService);

    conferenceSchedule = signal<ConferenceScheduleDto | null>(null);
    loading = signal(true);
    error = signal<string | null>(null);
    activeTabIndex = signal(0);

    scheduleDays = computed(() => {
        const schedule = this.conferenceSchedule();

        if (!schedule) {
            return [];
        }

        // Filter to only favorite presentations
        const favoritePresentations = schedule.presentations.filter(p => p.isFavorite);

        if (favoritePresentations.length === 0) {
            return [];
        }

        // Group presentations by day
        const dayMap = new Map<string, PresentationScheduleDto[]>();
        favoritePresentations.forEach((pres) => {
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

            // Find earliest and latest times
            const startTimes = dayPresentations.map((p) => new Date(p.startDateTime).getTime());
            const endTimes = dayPresentations.map((p) => new Date(p.endDateTime).getTime());
            const earliestTime = new Date(Math.min(...startTimes));
            const latestTime = new Date(Math.max(...endTimes));

            // Round start time down to the nearest hour
            const startTime = new Date(earliestTime);
            startTime.setMinutes(0, 0, 0);

            // Round end time up to the next hour
            const endTime = new Date(latestTime);
            endTime.setMinutes(0, 0, 0);
            if (latestTime.getTime() > endTime.getTime()) {
                endTime.setHours(endTime.getHours() + 1);
            }

            // Generate hours for the day
            const hours: Date[] = [];
            const currentHour = new Date(startTime);
            while (currentHour <= endTime) {
                hours.push(new Date(currentHour));
                currentHour.setHours(currentHour.getHours() + 1);
            }

            days.push({
                date,
                dateLabel: date.toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' }),
                presentations: dayPresentations,
                hours,
                startTime,
                endTime,
            });
        });

        return days;
    });

    ngOnInit(): void {
        const conferenceId = this.route.snapshot.paramMap.get('id');
        if (conferenceId) {
            this.loadData(conferenceId);
        } else {
            this.error.set('Conference ID not found');
            this.loading.set(false);
        }
    }

    loadData(id: string): void {
        this.loading.set(true);
        this.error.set(null);

        // Load conference schedule with presentations
        this.presenceService.getConferenceSchedule(id).subscribe({
            next: (schedule) => {
                this.conferenceSchedule.set(schedule);
                this.loading.set(false);
                this.determineActiveTab();
            },
            error: (err) => {
                this.error.set(err.status === 404 ? 'Conference not found' : 'Failed to load conference schedule');
                this.loading.set(false);
            },
        });
    }

    determineActiveTab(): void {
        const schedule = this.conferenceSchedule();
        if (!schedule) return;

        const days = this.scheduleDays();
        if (days.length === 0) {
            this.activeTabIndex.set(0);
            return;
        }

        const now = new Date();
        const confStart = new Date(schedule.startDate);
        const confEnd = new Date(schedule.endDate);

        if (now < confStart) {
            // Conference hasn't started - show first day
            this.activeTabIndex.set(0);
        } else if (now > confEnd) {
            // Conference is concluded - show last day
            this.activeTabIndex.set(days.length - 1);
        } else {
            // Conference is ongoing - find current day
            const todayKey = now.toDateString();
            const dayIndex = days.findIndex(d => d.date.toDateString() === todayKey);
            this.activeTabIndex.set(dayIndex >= 0 ? dayIndex : 0);
        }
    }

    getTimeSlots(day: ScheduleDay): TimeSlot[] {
        const slots: TimeSlot[] = [];
        const PIXELS_PER_HOUR = 120;

        day.hours.forEach((hour) => {
            const hourStart = hour.getTime();
            const hourEnd = hourStart + 60 * 60 * 1000;

            // Find presentations that overlap with this hour
            const overlappingPresentations = day.presentations.filter((p) => {
                const presStart = new Date(p.startDateTime).getTime();
                const presEnd = new Date(p.endDateTime).getTime();
                return presStart < hourEnd && presEnd > hourStart;
            });

            // Check if any of the presentations in this time slot is preferred
            const hasPreferred = overlappingPresentations.some(p =>
                p.isPreferred
            );

            const presentationInfos: PresentationInfo[] = overlappingPresentations.map((p) => {
                const presStart = new Date(p.startDateTime);
                const presEnd = new Date(p.endDateTime);

                // Calculate position within the hour
                const minutesFromHourStart = (presStart.getTime() - hourStart) / (1000 * 60);
                const startPixels = Math.max(0, (minutesFromHourStart / 60) * PIXELS_PER_HOUR);

                // Calculate duration in pixels
                const durationMinutes = (presEnd.getTime() - presStart.getTime()) / (1000 * 60);
                const heightPixels = (durationMinutes / 60) * PIXELS_PER_HOUR;

                const isPreferred = p.isPreferred;
                // If there's a preferred session in this slot and this isn't it, make it transparent
                const opacity = hasPreferred && !isPreferred ? 0.4 : 1;

                return {
                    presentation: p,
                    isFavorite: p.isFavorite,
                    isPreferred,
                    startPixels,
                    heightPixels,
                    opacity
                };
            });

            slots.push({
                hour,
                presentations: presentationInfos,
            });
        });

        return slots;
    }

    formatHour(date: Date): string {
        return date.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        });
    }

    formatTime(dateString: string): string {
        const date = new Date(dateString);
        return date.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        });
    }

    getSpeakerNames(presentation: PresentationScheduleDto): string {
        return presentation.speakers.map(s => s.name).join(', ');
    }
}
