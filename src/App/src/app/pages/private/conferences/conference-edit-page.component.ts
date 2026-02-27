import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TabsModule } from 'primeng/tabs';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConferencesService } from '@services/conferences.service';
import { ProfileStore } from '@stores/profile.store';
import { ConferenceDetailsDto } from '@models/conference-details-dto';
import { ConferenceSpeakerDto } from '@models/conference-speaker-dto';
import { ConferenceRoomDto } from '@models/conference-room-dto';
import { ConferencePresentationDto } from '@models/conference-presentation-dto';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'attn-conference-edit-page',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        CardModule,
        TabsModule,
        TableModule,
        DialogModule,
        InputTextModule,
        InputNumberModule,
        TextareaModule,
        ProgressSpinnerModule,
        MessageModule,
        MultiSelectModule,
        SelectModule,
        ConfirmDialogModule,
        ToastModule,
        TranslateModule,
    ],
    providers: [ConfirmationService, MessageService],
    templateUrl: './conference-edit-page.component.html',
    styleUrl: './conference-edit-page.component.scss',
})
export class ConferenceEditPageComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly conferencesService = inject(ConferencesService);
    private readonly confirmationService = inject(ConfirmationService);
    private readonly messageService = inject(MessageService);
    readonly profileStore = inject(ProfileStore);

    conferenceId = signal<string | null>(null);
    conference = signal<ConferenceDetailsDto | null>(null);
    loading = signal(true);
    error = signal<string | null>(null);
    accessDenied = signal(false);
    manualChangeWarning = signal(false);

    // Speakers
    speakers = signal<ConferenceSpeakerDto[]>([]);
    speakerDialogVisible = false;
    speakerForm = { name: '', company: '', profilePictureUrl: '', id: null as string | null };
    speakerSaving = false;

    // Rooms
    rooms = signal<ConferenceRoomDto[]>([]);
    roomDialogVisible = false;
    roomForm = { name: '', capacity: 50, id: null as string | null };
    roomSaving = false;

    // Presentations
    presentations = signal<ConferencePresentationDto[]>([]);
    presentationDialogVisible = false;
    presentationForm: {
        title: string;
        abstract: string;
        startDateTime: string;
        endDateTime: string;
        roomId: string;
        speakerIds: string[];
        id: string | null;
    } = { title: '', abstract: '', startDateTime: '', endDateTime: '', roomId: '', speakerIds: [], id: null };
    presentationSaving = false;

    roomOptions = computed(() =>
        this.rooms().map(r => ({ label: `${r.name} (cap: ${r.capacity})`, value: r.id }))
    );
    speakerOptions = computed(() =>
        this.speakers().map(s => ({ label: s.name + (s.company ? ` (${s.company})` : ''), value: s.id }))
    );

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (!id) {
            this.error.set('Conference ID not found');
            this.loading.set(false);
            return;
        }
        this.conferenceId.set(id);
        setTimeout(() => this.loadAll(id), 0);
    }

    private loadAll(id: string): void {
        this.loading.set(true);
        this.conferencesService.getConference(id).subscribe({
            next: (conf) => {
                this.conference.set(conf);
                this.loadSpeakers(id);
                this.loadRooms(id);
                this.loadPresentations(id);
                this.loading.set(false);
            },
            error: (err) => {
                this.error.set(err.status === 404 ? 'Conference not found' : 'Failed to load conference');
                this.loading.set(false);
            },
        });
    }

    private loadSpeakers(id: string): void {
        this.conferencesService.listSpeakers(id).subscribe({
            next: (s) => this.speakers.set(s),
            error: () => this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Failed to load speakers' }),
        });
    }

    private loadRooms(id: string): void {
        this.conferencesService.listRooms(id).subscribe({
            next: (r) => this.rooms.set(r),
            error: () => this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Failed to load rooms' }),
        });
    }

    private loadPresentations(id: string): void {
        this.conferencesService.listPresentations(id).subscribe({
            next: (p) => this.presentations.set(p),
            error: () => this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Failed to load presentations' }),
        });
    }

    navigateBack(): void {
        const id = this.conferenceId();
        if (id) {
            this.router.navigate(['/app/conferences', id]);
        } else {
            this.router.navigate(['/app/conferences']);
        }
    }

    // ── Speakers ──

    openCreateSpeakerDialog(): void {
        this.speakerForm = { name: '', company: '', profilePictureUrl: '', id: null };
        this.speakerDialogVisible = true;
    }

    openEditSpeakerDialog(speaker: ConferenceSpeakerDto): void {
        this.speakerForm = {
            name: speaker.name,
            company: speaker.company ?? '',
            profilePictureUrl: speaker.profilePictureUrl ?? '',
            id: speaker.id,
        };
        this.speakerDialogVisible = true;
    }

    saveSpeaker(): void {
        const id = this.conferenceId();
        if (!id) return;
        if (!this.speakerForm.name?.trim()) {
            this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Name is required' });
            return;
        }
        this.speakerSaving = true;
        const request = {
            name: this.speakerForm.name.trim(),
            company: this.speakerForm.company || undefined,
            profilePictureUrl: this.speakerForm.profilePictureUrl || undefined,
        };
        const obs = this.speakerForm.id
            ? this.conferencesService.updateSpeaker(id, this.speakerForm.id, request)
            : this.conferencesService.createSpeaker(id, request);
        obs.subscribe({
            next: (result) => {
                if (this.speakerForm.id) {
                    this.speakers.update(list => list.map(s => s.id === result.id ? result : s));
                } else {
                    this.speakers.update(list => [...list, result]);
                }
                this.speakerDialogVisible = false;
                this.speakerSaving = false;
                this.manualChangeWarning.set(true);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: this.speakerForm.id ? 'Speaker updated' : 'Speaker created' });
            },
            error: () => {
                this.speakerSaving = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save speaker' });
            },
        });
    }

    confirmDeleteSpeaker(speaker: ConferenceSpeakerDto): void {
        this.confirmationService.confirm({
            message: `Delete speaker "${speaker.name}"?`,
            header: 'Confirm Delete',
            icon: 'pi pi-exclamation-triangle',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => this.deleteSpeaker(speaker),
        });
    }

    private deleteSpeaker(speaker: ConferenceSpeakerDto): void {
        const id = this.conferenceId();
        if (!id) return;
        this.conferencesService.deleteSpeaker(id, speaker.id).subscribe({
            next: () => {
                this.speakers.update(list => list.filter(s => s.id !== speaker.id));
                this.manualChangeWarning.set(true);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Speaker deleted' });
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete speaker' }),
        });
    }

    // ── Rooms ──

    openCreateRoomDialog(): void {
        this.roomForm = { name: '', capacity: 50, id: null };
        this.roomDialogVisible = true;
    }

    openEditRoomDialog(room: ConferenceRoomDto): void {
        this.roomForm = { name: room.name, capacity: room.capacity, id: room.id };
        this.roomDialogVisible = true;
    }

    saveRoom(): void {
        const id = this.conferenceId();
        if (!id) return;
        if (!this.roomForm.name?.trim()) {
            this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Name is required' });
            return;
        }
        if (this.roomForm.capacity <= 0) {
            this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Capacity must be > 0' });
            return;
        }
        this.roomSaving = true;
        const request = { name: this.roomForm.name.trim(), capacity: this.roomForm.capacity };
        const obs = this.roomForm.id
            ? this.conferencesService.updateRoom(id, this.roomForm.id, request)
            : this.conferencesService.createRoom(id, request);
        obs.subscribe({
            next: (result) => {
                if (this.roomForm.id) {
                    this.rooms.update(list => list.map(r => r.id === result.id ? result : r));
                } else {
                    this.rooms.update(list => [...list, result]);
                }
                this.roomDialogVisible = false;
                this.roomSaving = false;
                this.manualChangeWarning.set(true);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: this.roomForm.id ? 'Room updated' : 'Room created' });
            },
            error: () => {
                this.roomSaving = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save room' });
            },
        });
    }

    confirmDeleteRoom(room: ConferenceRoomDto): void {
        this.confirmationService.confirm({
            message: `Delete room "${room.name}"?`,
            header: 'Confirm Delete',
            icon: 'pi pi-exclamation-triangle',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => this.deleteRoom(room),
        });
    }

    private deleteRoom(room: ConferenceRoomDto): void {
        const id = this.conferenceId();
        if (!id) return;
        this.conferencesService.deleteRoom(id, room.id).subscribe({
            next: () => {
                this.rooms.update(list => list.filter(r => r.id !== room.id));
                this.manualChangeWarning.set(true);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Room deleted' });
            },
            error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.error ?? 'Failed to delete room' }),
        });
    }

    // ── Presentations ──

    openCreatePresentationDialog(): void {
        this.presentationForm = { title: '', abstract: '', startDateTime: '', endDateTime: '', roomId: '', speakerIds: [], id: null };
        this.presentationDialogVisible = true;
    }

    openEditPresentationDialog(presentation: ConferencePresentationDto): void {
        this.presentationForm = {
            title: presentation.title,
            abstract: presentation.abstract,
            startDateTime: this.toDatetimeLocal(presentation.startDateTime),
            endDateTime: this.toDatetimeLocal(presentation.endDateTime),
            roomId: presentation.roomId,
            speakerIds: [...presentation.speakerIds],
            id: presentation.id,
        };
        this.presentationDialogVisible = true;
    }

    savePresentation(): void {
        const id = this.conferenceId();
        if (!id) return;
        if (!this.presentationForm.title?.trim()) {
            this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Title is required' });
            return;
        }
        if (!this.presentationForm.abstract?.trim()) {
            this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Abstract is required' });
            return;
        }
        if (!this.presentationForm.roomId) {
            this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Room is required' });
            return;
        }
        if (!this.presentationForm.speakerIds || this.presentationForm.speakerIds.length === 0) {
            this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'At least one speaker is required' });
            return;
        }
        const start = new Date(this.presentationForm.startDateTime);
        const end = new Date(this.presentationForm.endDateTime);
        if (end <= start) {
            this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'End must be after start' });
            return;
        }
        this.presentationSaving = true;
        const request = {
            title: this.presentationForm.title.trim(),
            abstract: this.presentationForm.abstract.trim(),
            startDateTime: start.toISOString(),
            endDateTime: end.toISOString(),
            roomId: this.presentationForm.roomId,
            speakerIds: this.presentationForm.speakerIds,
        };
        const formId = this.presentationForm.id;
        const obs = formId
            ? this.conferencesService.updatePresentation(id, formId, request)
            : this.conferencesService.createPresentation(id, request);
        obs.subscribe({
            next: (result) => {
                if (formId) {
                    this.presentations.update(list => list.map(p => p.id === result.id ? result : p));
                } else {
                    this.presentations.update(list => [...list, result]);
                }
                this.presentationDialogVisible = false;
                this.presentationSaving = false;
                this.manualChangeWarning.set(true);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: formId ? 'Presentation updated' : 'Presentation created' });
            },
            error: () => {
                this.presentationSaving = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save presentation' });
            },
        });
    }

    confirmDeletePresentation(presentation: ConferencePresentationDto): void {
        this.confirmationService.confirm({
            message: `Delete presentation "${presentation.title}"?`,
            header: 'Confirm Delete',
            icon: 'pi pi-exclamation-triangle',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => this.deletePresentation(presentation),
        });
    }

    private deletePresentation(presentation: ConferencePresentationDto): void {
        const id = this.conferenceId();
        if (!id) return;
        this.conferencesService.deletePresentation(id, presentation.id).subscribe({
            next: () => {
                this.presentations.update(list => list.filter(p => p.id !== presentation.id));
                this.manualChangeWarning.set(true);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Presentation deleted' });
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete presentation' }),
        });
    }

    private toDatetimeLocal(isoString: string): string {
        if (!isoString) return '';
        const d = new Date(isoString);
        const pad = (n: number) => String(n).padStart(2, '0');
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
    }

    formatDateTime(isoString: string): string {
        if (!isoString) return '';
        return new Date(isoString).toLocaleString();
    }
}
