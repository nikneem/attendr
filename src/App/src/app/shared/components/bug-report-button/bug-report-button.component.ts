import { Component, ViewChild } from '@angular/core';
import { PopoverModule, Popover } from 'primeng/popover';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

@Component({
    selector: 'attn-bug-report-button',
    imports: [PopoverModule, ButtonModule, TooltipModule],
    templateUrl: './bug-report-button.component.html',
    styleUrl: './bug-report-button.component.scss',
})
export class BugReportButtonComponent {
    @ViewChild('bugReportPanel') bugReportPanel?: Popover;

    readonly issuesUrl = 'https://github.com/nikneem/attendr/issues';

    toggleBugReport(event: Event): void {
        this.bugReportPanel?.toggle(event);
    }

    createIssue(): void {
        window.open(this.issuesUrl, '_blank');
    }
}
