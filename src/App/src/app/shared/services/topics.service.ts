import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TopicDto {
    id: string;
    conferenceId: string;
    key: string;
    name: string;
    isVisible: boolean;
}

@Injectable({
    providedIn: 'root',
})
export class TopicsService {
    private readonly apiUrl = `${environment.apiUrl}/topics`;

    constructor(private http: HttpClient) { }

    /**
     * Get all topics
     */
    getAllTopics(): Observable<TopicDto[]> {
        return this.http.get<TopicDto[]>(this.apiUrl);
    }

    /**
     * Get a specific topic by ID
     */
    getTopicById(id: string): Observable<TopicDto> {
        return this.http.get<TopicDto>(`${this.apiUrl}/${id}`);
    }

    /**
     * Create a new topic
     */
    createTopic(conferenceId: string, key: string, name: string): Observable<TopicDto> {
        return this.http.post<TopicDto>(this.apiUrl, {
            conferenceId,
            key,
            name,
        });
    }

    /**
     * Update an existing topic
     */
    updateTopic(id: string, key: string, name: string): Observable<TopicDto> {
        return this.http.put<TopicDto>(`${this.apiUrl}/${id}`, {
            key,
            name,
        });
    }

    /**
     * Delete a topic
     */
    deleteTopic(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}
