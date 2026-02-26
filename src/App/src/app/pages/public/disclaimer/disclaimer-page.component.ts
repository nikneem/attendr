import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TranslateModule } from '@ngx-translate/core';
@Component({
    selector: 'attn-disclaimer-page',
    standalone: true,
    imports: [CommonModule,
        TranslateModule
    ],
    templateUrl: './disclaimer-page.component.html',
    styleUrl: './disclaimer-page.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DisclaimerPageComponent { }
