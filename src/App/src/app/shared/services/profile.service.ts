import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateProfileRequest } from '../models/create-profile-request';
import { CreateProfileResult } from '../models/create-profile-result';

@Injectable({
    providedIn: 'root',
})
export class ProfileService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/profiles`;

    createProfile(request: CreateProfileRequest): Observable<CreateProfileResult> {
        return this.http.post<CreateProfileResult>(this.apiUrl, request);
    }
}
