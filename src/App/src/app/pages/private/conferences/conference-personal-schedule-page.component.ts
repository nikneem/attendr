import { Component, inject, OnInit, computed, signal, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Tabs, TabList, Tab, TabPanels, TabPanel } from 'primeng/tabs';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CardModule } from 'primeng/card';
import { TooltipModule } from 'primeng/tooltip';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { MenuItem } from 'primeng/api';
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
    rows: PresentationRow[];
}

interface PresentationRow {
    startTime: Date;
    endTime: Date;
    presentations: PresentationInfo[];
    hasPreferred: boolean;
    topPixels: number;
}

interface PresentationInfo {
    presentation: PresentationScheduleDto;
    isFavorite: boolean;
    isPreferred: boolean;
    startPixels: number;
    heightPixels: number;
    opacity: number;
    zIndex: number;
}

@Component({
    selector: 'attn-conference-personal-schedule-page',
    imports: [CommonModule, Tabs, TabList, Tab, TabPanels, TabPanel, ProgressSpinnerModule, CardModule, TooltipModule, ContextMenuModule],
    templateUrl: './conference-personal-schedule-page.component.html',
    styleUrl: './conference-personal-schedule-page.component.scss',
})
export class ConferencePersonalSchedulePageComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly presenceService = inject(PresenceService);

    @ViewChild('cm') contextMenu!: ContextMenu;

    conferenceSchedule = signal<ConferenceScheduleDto | null>(null);
    loading = signal(true);
    error = signal<string | null>(null);
    activeTabIndex = signal(0);
    contextMenuItems = signal<MenuItem[]>([]);
    selectedPresentation = signal<PresentationScheduleDto | null>(null);

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

    timeSlotsByDay = computed(() => {
        const days = this.scheduleDays();
        const PIXELS_PER_HOUR = 120;

        return days.map(day => {
            const slots: TimeSlot[] = [];

            day.hours.forEach((hour) => {
                const hourStart = hour.getTime();
                const hourEnd = hourStart + 60 * 60 * 1000;

                // Find presentations that START within this hour (not just overlap)
                const presentationsInHour = day.presentations.filter((p) => {
                    const presStart = new Date(p.startDateTime).getTime();
                    return presStart >= hourStart && presStart < hourEnd;
                });

                // Group presentations by their exact start and end times
                const timeGroups = new Map<string, PresentationScheduleDto[]>();
                presentationsInHour.forEach((p) => {
                    const key = `${p.startDateTime}_${p.endDateTime}`;
                    if (!timeGroups.has(key)) {
                        timeGroups.set(key, []);
                    }
                    timeGroups.get(key)!.push(p);
                });

                // Create rows for each unique time slot
                const rows: PresentationRow[] = [];
                timeGroups.forEach((presentations, key) => {
                    const startTime = new Date(presentations[0].startDateTime);
                    const endTime = new Date(presentations[0].endDateTime);

                    // Calculate top position based on minutes from hour start
                    const minutesFromHourStart = (startTime.getTime() - hourStart) / (1000 * 60);
                    const topPixels = Math.max(0, (minutesFromHourStart / 60) * PIXELS_PER_HOUR);

                    // Check if any presentation in this row is preferred
                    const hasPreferred = presentations.some(p => p.isPreferred);

                    const presentationInfos: PresentationInfo[] = presentations.map((p) => {
                        const presStart = new Date(p.startDateTime);
                        const presEnd = new Date(p.endDateTime);

                        // Calculate position within the hour
                        const minutesFromHourStart = (presStart.getTime() - hourStart) / (1000 * 60);
                        const startPixels = Math.max(0, (minutesFromHourStart / 60) * PIXELS_PER_HOUR);

                        // Calculate duration in pixels
                        const durationMinutes = (presEnd.getTime() - presStart.getTime()) / (1000 * 60);
                        const heightPixels = (durationMinutes / 60) * PIXELS_PER_HOUR;

                        const isPreferred = p.isPreferred;
                        // If there's a preferred session in this row and this isn't it, make it transparent
                        const opacity = hasPreferred && !isPreferred ? 0.4 : 1;

                        // Calculate z-index: earlier start times get higher z-index (inverse of minutes from hour start)
                        // Base z-index of 100, subtract minutes to make earlier sessions higher
                        const zIndex = 100 - Math.floor(minutesFromHourStart);

                        return {
                            presentation: p,
                            isFavorite: p.isFavorite,
                            isPreferred,
                            startPixels,
                            heightPixels,
                            opacity,
                            zIndex
                        };
                    });

                    // Sort presentations: preferred first, then by presentation order
                    presentationInfos.sort((a, b) => {
                        if (a.isPreferred && !b.isPreferred) return -1;
                        if (!a.isPreferred && b.isPreferred) return 1;
                        return 0;
                    });

                    rows.push({
                        startTime,
                        endTime,
                        presentations: presentationInfos,
                        hasPreferred,
                        topPixels
                    });
                });

                // Sort rows by start time
                rows.sort((a, b) => a.startTime.getTime() - b.startTime.getTime());

                slots.push({
                    hour,
                    rows
                });
            });

            return {
                day,
                slots
            };
        });
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

    onPresentationClick(event: MouseEvent, presentation: PresentationScheduleDto, row: PresentationRow): void {
        event.preventDefault();
        this.selectedPresentation.set(presentation);

        const canCheckIn = this.canCheckIn(presentation);
        const canSetPreferred = this.canSetPreferred(presentation, row);

        this.contextMenuItems.set([
            {
                label: 'Check In',
                icon: 'pi pi-check-circle',
                disabled: !canCheckIn,
                command: () => this.checkInToPresentation(presentation)
            },
            {
                label: 'Set as Preferred',
                icon: 'pi pi-heart',
                disabled: !canSetPreferred,
                command: () => this.setPreferredPresentation(presentation)
            }
        ]);

        this.contextMenu.show(event);
    }

    canCheckIn(presentation: PresentationScheduleDto): boolean {
        const now = new Date();
        const startTime = new Date(presentation.startDateTime);
        const thirtyMinutesBefore = new Date(startTime.getTime() - 30 * 60 * 1000);

        return now >= thirtyMinutesBefore;
    }

    canSetPreferred(presentation: PresentationScheduleDto, row: PresentationRow): boolean {
        // Can only set preferred if there are multiple presentations in the same time slot
        return row.presentations.length > 1;
    }

    checkInToPresentation(presentation: PresentationScheduleDto): void {
        // TODO: Implement check-in logic
        console.log('Checking in to:', presentation.title);
    }

    setPreferredPresentation(presentation: PresentationScheduleDto): void {
        const schedule = this.conferenceSchedule();
        if (!schedule) return;

        this.presenceService.setPreferredPresentation(schedule.conferenceId, presentation.presentationId).subscribe({
            next: () => {
                // Reload the schedule to get updated preferred status
                this.loadData(schedule.conferenceId);
            },
            error: (err) => {
                console.error('Failed to set preferred presentation:', err);
                // TODO: Show error message to user
            }
        });
    }
}
