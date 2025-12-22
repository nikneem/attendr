import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { GroupListItemDto } from '../../models/group-list-item-dto';

@Component({
    selector: 'attn-group-details-view',
    imports: [CommonModule, ButtonModule, TagModule, DividerModule],
    templateUrl: './group-details-view.component.html',
    styleUrl: './group-details-view.component.scss',
})
export class GroupDetailsViewComponent {
    group = input.required<GroupListItemDto>();

    joinRequested = output<string>();
    requestAccessRequested = output<string>();

    onJoin(): void {
        this.joinRequested.emit(this.group().id);
    }

    onRequestAccess(): void {
        this.requestAccessRequested.emit(this.group().id);
    }
}
