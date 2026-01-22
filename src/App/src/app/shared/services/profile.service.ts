import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateProfileRequest } from '../models/create-profile-request';
import { CreateProfileResult } from '../models/create-profile-result';
import { ProfileDetailsDto } from '../models/profile-details-dto';
import { UpdateProfileRequest } from '../models/update-profile-request';
import { UpdateProfileResult } from '../models/update-profile-result';
import { ProfileTopicDto } from '../models/profile-topic-dto';

@Injectable({
    providedIn: 'root',
})
export class ProfileService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/profiles`;

    createProfile(request: CreateProfileRequest): Observable<CreateProfileResult> {
        return this.http.post<CreateProfileResult>(this.apiUrl, request);
    }

    getProfileDetails(): Observable<ProfileDetailsDto> {
        return this.http.get<ProfileDetailsDto>(this.apiUrl);
    }

    updateProfile(request: UpdateProfileRequest): Observable<UpdateProfileResult> {
        return this.http.put<UpdateProfileResult>(this.apiUrl, request);
    }

    getProfileTopics(): Observable<ProfileTopicDto[]> {
        return this.http.get<ProfileTopicDto[]>(`${this.apiUrl}/topics`);
    }
}
