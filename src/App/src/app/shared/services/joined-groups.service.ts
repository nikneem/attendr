import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MyGroupDto } from '../models/my-group-dto';

@Injectable({
    providedIn: 'root',
})
export class JoinedGroupsService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/groups`;

    getMyGroups(): Observable<MyGroupDto[]> {
        return this.http.get<MyGroupDto[]>(`${this.apiUrl}/my-groups`);
    }
}
