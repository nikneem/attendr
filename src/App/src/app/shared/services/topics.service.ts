import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TopicDto {
    id: string;
    key: string;
    name: string;
    isVisible: boolean;
}

interface ListTopicsResult {
    topics: TopicDto[];
    totalCount: number;
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
        return this.http.get<ListTopicsResult>(this.apiUrl).pipe(
            map((result) => result.topics)
        );
    }

    /**
     * Get a specific topic by ID
     */
    getTopicById(id: string): Observable<TopicDto> {
        return this.http.get<TopicDto>(`${this.apiUrl}/${id}`);
    }

    /**
     * Create a new topic manually (visible by default)
     */
    createTopic(key: string, name: string): Observable<TopicDto> {
        return this.http.post<TopicDto>(this.apiUrl, {
            key,
            name,
        });
    }

    /**
     * Update an existing topic
     */
    updateTopic(id: string, key: string, name: string, isVisible: boolean): Observable<TopicDto> {
        return this.http.put<TopicDto>(`${this.apiUrl}/${id}`, {
            key,
            name,
            isVisible,
        });
    }

    /**
     * Delete a topic
     */
    deleteTopic(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}
