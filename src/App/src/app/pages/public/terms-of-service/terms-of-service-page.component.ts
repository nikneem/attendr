import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TranslateModule } from '@ngx-translate/core';
@Component({
    selector: 'attn-terms-of-service-page',
    standalone: true,
    imports: [CommonModule,
        TranslateModule
    ],
    templateUrl: './terms-of-service-page.component.html',
    styleUrl: './terms-of-service-page.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TermsOfServicePageComponent { }
