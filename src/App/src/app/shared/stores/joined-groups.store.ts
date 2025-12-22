import { Injectable, signal, inject } from '@angular/core';
import { MyGroupDto } from '../models/my-group-dto';
import { JoinedGroupsService } from '../services/joined-groups.service';

@Injectable({
    providedIn: 'root',
})
export class JoinedGroupsStore {
    private readonly joinedGroupsService = inject(JoinedGroupsService);

    private readonly _groups = signal<MyGroupDto[]>([]);
    private readonly _loading = signal<boolean>(false);
    private readonly _error = signal<string | null>(null);

    readonly groups = this._groups.asReadonly();
    readonly loading = this._loading.asReadonly();
    readonly error = this._error.asReadonly();

    loadGroups(): void {
        this._loading.set(true);
        this._error.set(null);

        this.joinedGroupsService.getMyGroups().subscribe({
            next: (groups) => {
                this._groups.set(groups);
                this._loading.set(false);
            },
            error: (err) => {
                this._error.set(err.message || 'Failed to load groups');
                this._loading.set(false);
            },
        });
    }

    refresh(): void {
        this.loadGroups();
    }

    addGroup(group: { id: string; name: string; memberCount: number }): void {
        const newGroup: MyGroupDto = {
            id: group.id,
            name: group.name,
            memberCount: group.memberCount,
        };
        this._groups.update(groups => [...groups, newGroup]);
    }
}
