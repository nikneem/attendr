import { Component, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'attn-home-page-component',
  imports: [],
  templateUrl: './home-page-component.html',
  styleUrl: './home-page-component.scss',
})
export class HomePageComponent implements AfterViewInit, OnDestroy {
  private observer!: IntersectionObserver;
  private statsAnimated = false;

  stats = [
    { value: 5, suffix: '+', label: 'Conferences', current: 0 },
    { value: 35, suffix: '+', label: 'Attendees', current: 0 },
    { value: 98, suffix: '%', label: 'Satisfaction', current: 0 },
    { value: 121, suffix: '+', label: 'Sessions Tracked', current: 0 },
  ];

  constructor(
    private oidcSecurityService: OidcSecurityService,
    private cdr: ChangeDetectorRef
  ) {}

  ngAfterViewInit() {
    this.setupScrollReveal();
  }

  ngOnDestroy() {
    if (this.observer) {
      this.observer.disconnect();
    }
  }

  login() {
    this.oidcSecurityService.authorize();
  }

  scrollToFeatures() {
    document.getElementById('features')?.scrollIntoView({ behavior: 'smooth' });
  }

  private setupScrollReveal() {
    this.observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('revealed');

            if (entry.target.classList.contains('stats-section') && !this.statsAnimated) {
              this.statsAnimated = true;
              this.animateCounters();
            }

            this.observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.15 }
    );

    document.querySelectorAll('.reveal').forEach((el) => this.observer.observe(el));
  }

  private animateCounters() {
    const duration = 2000;
    const startTime = performance.now();

    const animate = (currentTime: number) => {
      const progress = Math.min((currentTime - startTime) / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3);

      this.stats.forEach((stat) => {
        stat.current = Math.floor(eased * stat.value);
      });

      this.cdr.detectChanges();

      if (progress < 1) {
        requestAnimationFrame(animate);
      }
    };

    requestAnimationFrame(animate);
  }
}
