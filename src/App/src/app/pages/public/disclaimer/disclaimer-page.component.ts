import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'attn-disclaimer-page',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './disclaimer-page.component.html',
    styleUrl: './disclaimer-page.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DisclaimerPageComponent { }
