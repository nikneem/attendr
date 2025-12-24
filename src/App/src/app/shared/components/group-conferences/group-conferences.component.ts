import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { GroupDetailsStore } from '../../stores/group-details.store';

@Component({
    selector: 'attn-group-conferences',
    standalone: true,
    imports: [CommonModule, CardModule, TagModule],
    templateUrl: './group-conferences.component.html',
    styleUrl: './group-conferences.component.scss',
})
export class GroupConferencesComponent {
    private readonly store = inject(GroupDetailsStore);

    group = this.store.groupDetails;
    conferences = computed(() => this.group()?.followedConferences ?? []);
}
