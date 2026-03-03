import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { DatePickerModule } from 'primeng/datepicker';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TabsModule } from 'primeng/tabs';
import { MessageModule } from 'primeng/message';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConferencesService } from '@services/conferences.service';
import { ConferenceSpeakersService, ConferenceSpeakerDto, CreateConferenceSpeakerRequest } from '@services/conference-speakers.service';
import { ConferenceRoomsService, ConferenceRoomDto, CreateConferenceRoomRequest } from '@services/conference-rooms.service';
import { ConferencePresentationsService, ConferencePresentationDto, CreateConferencePresentationRequest } from '@services/conference-presentations.service';
import { ProfileStore } from '@stores/profile.store';
import { ConferenceDetailsDto } from '@models/conference-details-dto';

@Component({
    selector: 'attn-conference-edit-page',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        TableModule,
        DialogModule,
        InputTextModule,
        TextareaModule,
        InputNumberModule,
        SelectModule,
        MultiSelectModule,
        DatePickerModule,
        ProgressSpinnerModule,
        TabsModule,
        MessageModule,
        ConfirmDialogModule,
        TooltipModule,
    ],
    providers: [ConfirmationService],
    templateUrl: './conference-edit-page.component.html',
    styleUrl: './conference-edit-page.component.scss',
})
export class ConferenceEditPageComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly conferencesService = inject(ConferencesService);
    private readonly speakersService = inject(ConferenceSpeakersService);
    private readonly roomsService = inject(ConferenceRoomsService);
    private readonly presentationsService = inject(ConferencePresentationsService);
    readonly profileStore = inject(ProfileStore);
    private readonly messageService = inject(MessageService);
    private readonly confirmationService = inject(ConfirmationService);

    conferenceId = signal<string>('');
    conference = signal<ConferenceDetailsDto | null>(null);
    loading = signal(true);
    error = signal<string | null>(null);
    accessDenied = signal(false);

    // Speakers state
    speakers = signal<ConferenceSpeakerDto[]>([]);
    speakersLoading = signal(false);
    showSpeakerDialog = signal(false);
    editingSpeaker = signal<ConferenceSpeakerDto | null>(null);
    speakerForm = signal({ name: '', company: '', profilePictureUrl: '' });
    speakerSaving = signal(false);

    // Rooms state
    rooms = signal<ConferenceRoomDto[]>([]);
    roomsLoading = signal(false);
    showRoomDialog = signal(false);
    editingRoom = signal<ConferenceRoomDto | null>(null);
    roomForm = signal({ name: '', capacity: 1 });
    roomSaving = signal(false);

    // Presentations state
    presentations = signal<ConferencePresentationDto[]>([]);
    presentationsLoading = signal(false);
    showPresentationDialog = signal(false);
    editingPresentation = signal<ConferencePresentationDto | null>(null);
    presentationForm = signal({
        title: '',
        abstract: '',
        startDateTime: null as Date | null,
        endDateTime: null as Date | null,
        roomId: null as string | null,
        speakerIds: [] as string[],
    });
    presentationSaving = signal(false);

    manualChangesWarning = signal(false);

    availableRoomsOptions = computed(() =>
        this.rooms().map(r => ({ label: r.name, value: r.id }))
    );

    availableSpeakersOptions = computed(() =>
        this.speakers().map(s => ({ label: s.name, value: s.id }))
    );

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (!id) {
            this.error.set('Conference ID not found');
            this.loading.set(false);
            return;
        }
        this.conferenceId.set(id);
        this.loadConference(id);
    }

    private loadConference(id: string): void {
        this.loading.set(true);
        this.conferencesService.getConference(id).pipe(finalize(() => this.loading.set(false))).subscribe({
            next: (conf) => {
                this.conference.set(conf);
                this.loadSpeakers(id);
                this.loadRooms(id);
                this.loadPresentations(id);
            },
            error: (err) => {
                if (err.status === 403) {
                    this.accessDenied.set(true);
                } else {
                    this.error.set(err.status === 404 ? 'Conference not found' : 'Failed to load conference');
                }
            },
        });
    }

    loadSpeakers(id?: string): void {
        const conferenceId = id ?? this.conferenceId();
        this.speakersLoading.set(true);
        this.speakersService.listSpeakers(conferenceId).pipe(finalize(() => this.speakersLoading.set(false))).subscribe({
            next: (s) => this.speakers.set(s),
            error: (err) => {
                if (err.status === 403) this.accessDenied.set(true);
                else this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load speakers' });
            },
        });
    }

    loadRooms(id?: string): void {
        const conferenceId = id ?? this.conferenceId();
        this.roomsLoading.set(true);
        this.roomsService.listRooms(conferenceId).pipe(finalize(() => this.roomsLoading.set(false))).subscribe({
            next: (r) => this.rooms.set(r),
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load rooms' }),
        });
    }

    loadPresentations(id?: string): void {
        const conferenceId = id ?? this.conferenceId();
        this.presentationsLoading.set(true);
        this.presentationsService.listPresentations(conferenceId).pipe(finalize(() => this.presentationsLoading.set(false))).subscribe({
            next: (p) => this.presentations.set(p),
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load presentations' }),
        });
    }

    // Speaker operations
    openAddSpeaker(): void {
        this.editingSpeaker.set(null);
        this.speakerForm.set({ name: '', company: '', profilePictureUrl: '' });
        this.showSpeakerDialog.set(true);
    }

    openEditSpeaker(speaker: ConferenceSpeakerDto): void {
        this.editingSpeaker.set(speaker);
        this.speakerForm.set({ name: speaker.name, company: speaker.company ?? '', profilePictureUrl: speaker.profilePictureUrl ?? '' });
        this.showSpeakerDialog.set(true);
    }

    saveSpeaker(): void {
        const form = this.speakerForm();
        if (!form.name.trim()) return;

        const request: CreateConferenceSpeakerRequest = {
            name: form.name.trim(),
            company: form.company.trim() || undefined,
            profilePictureUrl: form.profilePictureUrl.trim() || undefined,
        };

        this.speakerSaving.set(true);
        const editing = this.editingSpeaker();
        const op = editing
            ? this.speakersService.updateSpeaker(this.conferenceId(), editing.id, request)
            : this.speakersService.createSpeaker(this.conferenceId(), request);

        op.pipe(finalize(() => this.speakerSaving.set(false))).subscribe({
            next: () => {
                this.showSpeakerDialog.set(false);
                this.loadSpeakers();
                this.manualChangesWarning.set(true);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: editing ? 'Speaker updated' : 'Speaker added' });
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save speaker' }),
        });
    }

    confirmDeleteSpeaker(speaker: ConferenceSpeakerDto): void {
        this.confirmationService.confirm({
            message: `Are you sure you want to delete speaker "${speaker.name}"?`,
            header: 'Delete Speaker',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.speakersService.deleteSpeaker(this.conferenceId(), speaker.id).subscribe({
                    next: () => {
                        this.loadSpeakers();
                        this.manualChangesWarning.set(true);
                        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Speaker deleted' });
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete speaker' }),
                });
            },
        });
    }

    // Room operations
    openAddRoom(): void {
        this.editingRoom.set(null);
        this.roomForm.set({ name: '', capacity: 1 });
        this.showRoomDialog.set(true);
    }

    openEditRoom(room: ConferenceRoomDto): void {
        this.editingRoom.set(room);
        this.roomForm.set({ name: room.name, capacity: room.capacity });
        this.showRoomDialog.set(true);
    }

    saveRoom(): void {
        const form = this.roomForm();
        if (!form.name.trim() || form.capacity <= 0) return;

        const request: CreateConferenceRoomRequest = { name: form.name.trim(), capacity: form.capacity };
        this.roomSaving.set(true);
        const editing = this.editingRoom();
        const op = editing
            ? this.roomsService.updateRoom(this.conferenceId(), editing.id, request)
            : this.roomsService.createRoom(this.conferenceId(), request);

        op.pipe(finalize(() => this.roomSaving.set(false))).subscribe({
            next: () => {
                this.showRoomDialog.set(false);
                this.loadRooms();
                this.manualChangesWarning.set(true);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: editing ? 'Room updated' : 'Room added' });
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save room' }),
        });
    }

    confirmDeleteRoom(room: ConferenceRoomDto): void {
        this.confirmationService.confirm({
            message: `Are you sure you want to delete room "${room.name}"?`,
            header: 'Delete Room',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.roomsService.deleteRoom(this.conferenceId(), room.id).subscribe({
                    next: () => {
                        this.loadRooms();
                        this.manualChangesWarning.set(true);
                        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Room deleted' });
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete room' }),
                });
            },
        });
    }

    // Presentation operations
    openAddPresentation(): void {
        this.editingPresentation.set(null);
        this.presentationForm.set({ title: '', abstract: '', startDateTime: null, endDateTime: null, roomId: null, speakerIds: [] });
        this.showPresentationDialog.set(true);
    }

    openEditPresentation(presentation: ConferencePresentationDto): void {
        this.editingPresentation.set(presentation);
        this.presentationForm.set({
            title: presentation.title,
            abstract: presentation.abstract,
            startDateTime: new Date(presentation.startDateTime),
            endDateTime: new Date(presentation.endDateTime),
            roomId: presentation.roomId,
            speakerIds: [...presentation.speakerIds],
        });
        this.showPresentationDialog.set(true);
    }

    savePresentation(): void {
        const form = this.presentationForm();
        if (!form.title.trim() || !form.abstract.trim() || !form.startDateTime || !form.endDateTime || !form.roomId) return;

        const request: CreateConferencePresentationRequest = {
            title: form.title.trim(),
            abstract: form.abstract.trim(),
            startDateTime: form.startDateTime.toISOString(),
            endDateTime: form.endDateTime.toISOString(),
            roomId: form.roomId,
            speakerIds: form.speakerIds,
        };

        this.presentationSaving.set(true);
        const editing = this.editingPresentation();
        const op = editing
            ? this.presentationsService.updatePresentation(this.conferenceId(), editing.id, request)
            : this.presentationsService.createPresentation(this.conferenceId(), request);

        op.pipe(finalize(() => this.presentationSaving.set(false))).subscribe({
            next: () => {
                this.showPresentationDialog.set(false);
                this.loadPresentations();
                this.manualChangesWarning.set(true);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: editing ? 'Presentation updated' : 'Presentation added' });
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save presentation' }),
        });
    }

    confirmDeletePresentation(presentation: ConferencePresentationDto): void {
        this.confirmationService.confirm({
            message: `Are you sure you want to delete presentation "${presentation.title}"?`,
            header: 'Delete Presentation',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.presentationsService.deletePresentation(this.conferenceId(), presentation.id).subscribe({
                    next: () => {
                        this.loadPresentations();
                        this.manualChangesWarning.set(true);
                        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Presentation deleted' });
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete presentation' }),
                });
            },
        });
    }

    formatDateTime(dt: string): string {
        return new Date(dt).toLocaleString('en-US', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
    }

    navigateBack(): void {
        this.router.navigate(['/app/conferences', this.conferenceId()]);
    }

    updateSpeakerFormField(field: keyof ReturnType<typeof this.speakerForm>, value: string): void {
        this.speakerForm.update(f => ({ ...f, [field]: value }));
    }

    updateRoomFormField(field: 'name' | 'capacity', value: string | number): void {
        this.roomForm.update(f => ({ ...f, [field]: value }));
    }

    updatePresentationFormField(field: string, value: unknown): void {
        this.presentationForm.update(f => ({ ...f, [field]: value }));
    }
}
