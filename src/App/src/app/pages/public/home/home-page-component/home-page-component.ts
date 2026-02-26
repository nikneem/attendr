import { Component, AfterViewInit, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { forkJoin } from 'rxjs';
import { ConferencesService } from '../../../../shared/services/conferences.service';
import { ProfileService } from '../../../../shared/services/profile.service';

interface StatItem {
  value: number;
  suffix: string;
  label: string;
  current: number;
}

import { TranslateModule } from '@ngx-translate/core';
@Component({
  selector: 'attn-home-page-component',
  imports: [TranslateModule],
  templateUrl: './home-page-component.html',
  styleUrl: './home-page-component.scss',
})
export class HomePageComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private readonly conferencesService = inject(ConferencesService);
  private readonly profileService = inject(ProfileService);

  private observer!: IntersectionObserver;
  private statsVisible = false;
  private statsDataLoaded = false;
  private statsAnimated = false;

  stats = signal<StatItem[]>([
    { value: 0, suffix: '+', label: 'Conferences', current: 0 },
    { value: 0, suffix: '+', label: 'Attendees', current: 0 },
    { value: 98, suffix: '%', label: 'Satisfaction', current: 0 },
    { value: 0, suffix: '+', label: 'Sessions Tracked', current: 0 },
  ]);

  ngOnInit() {
    forkJoin({
      conferenceMetrics: this.conferencesService.getMetrics(),
      profileMetrics: this.profileService.getMetrics(),
    }).subscribe(({ conferenceMetrics, profileMetrics }) => {
      this.stats.set([
        { value: conferenceMetrics.visibleConferences, suffix: '+', label: 'Conferences', current: 0 },
        { value: profileMetrics.totalUsers, suffix: '+', label: 'Attendees', current: 0 },
        { value: 98, suffix: '%', label: 'Satisfaction', current: 0 },
        { value: conferenceMetrics.totalPresentations, suffix: '+', label: 'Sessions Tracked', current: 0 },
      ]);
      this.statsDataLoaded = true;
      this.maybeAnimateCounters();
    });
  }

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

            if (entry.target.classList.contains('stats-section')) {
              this.statsVisible = true;
              this.maybeAnimateCounters();
            }

            this.observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.15 }
    );

    document.querySelectorAll('.reveal').forEach((el) => this.observer.observe(el));
  }

  private maybeAnimateCounters() {
    if (this.statsVisible && this.statsDataLoaded && !this.statsAnimated) {
      this.statsAnimated = true;
      this.animateCounters();
    }
  }

  private animateCounters() {
    const duration = 2000;
    const startTime = performance.now();
    const targets = this.stats().map((s) => s.value);

    const animate = (currentTime: number) => {
      const progress = Math.min((currentTime - startTime) / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3);

      this.stats.update((stats) =>
        stats.map((stat, i) => ({ ...stat, current: Math.floor(eased * targets[i]) }))
      );

      if (progress < 1) {
        requestAnimationFrame(animate);
      }
    };

    requestAnimationFrame(animate);
  }
}
