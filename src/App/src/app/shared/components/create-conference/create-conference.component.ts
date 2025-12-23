import { Component, inject, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DatePickerModule } from 'primeng/datepicker';
import { CheckboxModule } from 'primeng/checkbox';
import { SelectModule } from 'primeng/select';
import { ConferencesService } from '../../services/conferences.service';
import { MessageService } from 'primeng/api';
import { CreateConferenceRequest } from '../../models/create-conference-request.model';

@Component({
    selector: 'attn-create-conference',
    imports: [FormsModule, ButtonModule, InputTextModule, DatePickerModule, CheckboxModule, SelectModule],
    templateUrl: './create-conference.component.html',
    styleUrl: './create-conference.component.scss',
})
export class CreateConferenceComponent {
    private readonly conferencesService = inject(ConferencesService);
    private readonly messageService = inject(MessageService);

    title = '';
    city = '';
    country = '';
    imageUrl = '';
    dateRange: Date[] | null = null;

    hasSyncSource = false;
    syncSourceType = '';
    syncSourceUrl = '';

    sourceTypeOptions = [
        { label: 'Sessionize', value: 'Sessionize' }
    ];

    isCreating = false;
    validationError = '';

    conferenceCreated = output<{ id: string; title: string }>();

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

        this.isCreating = true;

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

        this.conferencesService.createConference(request).subscribe({
            next: (result) => {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: `Conference "${result.title}" created successfully`,
                });

                this.conferenceCreated.emit({
                    id: result.id,
                    title: result.title,
                });

                this.resetForm();
                this.isCreating = false;
            },
            error: (error) => {
                this.isCreating = false;
                const errorMessage = error.error?.error || 'Failed to create conference';
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

    private resetForm() {
        this.title = '';
        this.city = '';
        this.country = '';
        this.imageUrl = '';
        this.dateRange = null;
        this.hasSyncSource = false;
        this.syncSourceType = '';
        this.syncSourceUrl = '';
        this.validationError = '';
    }
}
