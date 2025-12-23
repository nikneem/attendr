import { Component, inject, input, output, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DatePickerModule } from 'primeng/datepicker';
import { CheckboxModule } from 'primeng/checkbox';
import { SelectModule } from 'primeng/select';
import { ConferencesService } from '../../services/conferences.service';
import { MessageService } from 'primeng/api';
import { CreateConferenceRequest } from '../../models/create-conference-request.model';
import { ConferenceDetailsDto } from '../../models/conference-details-dto';

@Component({
    selector: 'attn-edit-conference',
    imports: [FormsModule, ButtonModule, InputTextModule, DatePickerModule, CheckboxModule, SelectModule],
    templateUrl: './edit-conference.component.html',
    styleUrl: './edit-conference.component.scss',
})
export class EditConferenceComponent {
    private readonly conferencesService = inject(ConferencesService);
    private readonly messageService = inject(MessageService);

    conference = input.required<ConferenceDetailsDto>();

    title = '';
    city = '';
    country = '';
    imageUrl = '';
    dateRange: Date[] | null = null;

    hasSyncSource = false;
    syncSourceType = '';
    syncSourceUrl = '';

    sourceTypeOptions = [{ label: 'Sessionize', value: 'Sessionize' }];

    isUpdating = false;
    validationError = '';

    conferenceUpdated = output<void>();

    constructor() {
        effect(() => {
            const conf = this.conference();
            if (conf) {
                this.initializeForm(conf);
            }
        });
    }

    private initializeForm(conf: ConferenceDetailsDto): void {
        this.title = conf.title;
        this.city = conf.city || '';
        this.country = conf.country || '';
        this.imageUrl = conf.imageUrl || '';
        this.dateRange = [new Date(conf.startDate), new Date(conf.endDate)];

        if (conf.synchronizationSource) {
            this.hasSyncSource = true;
            this.syncSourceType = conf.synchronizationSource.sourceType;
            this.syncSourceUrl = conf.synchronizationSource.sourceUrl;
        } else {
            this.hasSyncSource = false;
            this.syncSourceType = '';
            this.syncSourceUrl = '';
        }
    }

    validateForm(): boolean {
        this.validationError = '';

        if (!this.title.trim()) {
            this.validationError = 'Conference title is required';
            return false;
        }

        if (this.title.trim().length < 3) {
            this.validationError = 'Conference title must be at least 3 characters long';
            return false;
        }

        if (this.title.trim().length > 200) {
            this.validationError = 'Conference title must not exceed 200 characters';
            return false;
        }

        if (!this.dateRange || this.dateRange.length === 0) {
            this.validationError = 'Date range is required';
            return false;
        }

        if (this.dateRange.length === 1) {
            this.validationError = 'Please select both start and end dates';
            return false;
        }

        if (this.imageUrl.trim()) {
            try {
                new URL(this.imageUrl.trim());
            } catch {
                this.validationError = 'Please enter a valid URL for the image';
                return false;
            }
        }

        if (this.hasSyncSource) {
            if (!this.syncSourceType.trim()) {
                this.validationError = 'Synchronization source type is required';
                return false;
            }

            if (!this.syncSourceUrl.trim()) {
                this.validationError = 'API Key is required';
                return false;
            }

            if (this.syncSourceUrl.trim().length > 256) {
                this.validationError = 'API Key must not exceed 256 characters';
                return false;
            }
        }

        return true;
    }

    onSubmit() {
        if (!this.validateForm()) {
            return;
        }

        this.isUpdating = true;

        const request: CreateConferenceRequest = {
            title: this.title.trim(),
            city: this.city.trim() || undefined,
            country: this.country.trim() || undefined,
            imageUrl: this.imageUrl.trim() || undefined,
            startDate: this.formatDate(this.dateRange![0]),
            endDate: this.formatDate(this.dateRange![1]),
        };

        if (this.hasSyncSource) {
            request.synchronizationSource = {
                sourceType: this.syncSourceType.trim(),
                sourceUrl: this.syncSourceUrl.trim(),
            };
        }

        this.conferencesService.updateConference(this.conference().id, request).subscribe({
            next: () => {
                this.conferenceUpdated.emit();
                this.isUpdating = false;
            },
            error: (error) => {
                this.isUpdating = false;
                const errorMessage = error.error?.error || 'Failed to update conference';
                this.validationError = errorMessage;
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: errorMessage,
                });
            },
        });
    }

    private formatDate(date: Date): string {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }
}
