import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';
import { FooterComponent } from './shared/footer/footer.component';

@Component({
  selector: 'attn-root',
  imports: [RouterOutlet, ToastModule, FooterComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('attendr');
}
