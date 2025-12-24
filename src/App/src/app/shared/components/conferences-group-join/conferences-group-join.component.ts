import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { ConferencesService } from '../../services/conferences.service';
import { GroupDetailsStore } from '../../stores/group-details.store';
import { ConferenceListItemDto } from '../../models/conference-list-item-dto';

interface ConferenceViewModel {
  id: string;
  title: string;
  city: string;
  country: string;
  startDate: string;
  endDate: string;
  speakersCount?: number;
  presentationsCount?: number;
}

@Component({
  selector: 'attn-conferences-group-join',
  standalone: true,
  imports: [CommonModule, CardModule, ButtonModule, TagModule, ProgressSpinnerModule, TooltipModule],
  templateUrl: './conferences-group-join.component.html',
  styleUrl: './conferences-group-join.component.scss',
})
export class ConferencesGroupJoinComponent implements OnInit {
  private readonly conferencesService = inject(ConferencesService);
  private readonly store = inject(GroupDetailsStore);

  loading = signal(true);
  error = signal<string | null>(null);
  followingIds = signal<Set<string>>(new Set());

  // Source data
  private all: ConferenceListItemDto[] = [];
  items = signal<ConferenceViewModel[]>([]);

  // From store
  group = this.store.groupDetails;
  followed = computed(() => this.group()?.followedConferences?.map(fc => fc.id) ?? []);
  isAlreadyFollowed = (id: string) => this.followed().includes(id);

  ngOnInit(): void {
    this.loadConferences();
  }

  private loadConferences(): void {
    this.loading.set(true);
    this.error.set(null);

    this.conferencesService.listConferences(undefined, 100, 1).subscribe({
      next: (result) => {
        this.all = result.conferences || [];
        const now = new Date();
        const filtered = this.all
          .filter(c => new Date(c.endDate) >= now)
          .sort((a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime())
          .map<ConferenceViewModel>(c => ({
            id: c.id,
            title: c.title,
            city: c.city,
            country: c.country,
            startDate: c.startDate,
            endDate: c.endDate,
          }));
        this.items.set(filtered);
        this.loading.set(false);
        // Fetch counts in background
        this.prefetchCounts(filtered.map(f => f.id));
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load conferences');
        this.loading.set(false);
      }
    });
  }

  private prefetchCounts(ids: string[]): void {
    // Best-effort: fetch details to compute counts
    ids.forEach(id => {
      this.conferencesService.getConference(id).subscribe({
        next: (details) => {
          const updated = this.items().map(item => item.id === id
            ? { ...item, speakersCount: details.speakers?.length ?? 0, presentationsCount: details.presentations?.length ?? 0 }
            : item);
          this.items.set(updated);
        },
        error: () => {
          // Ignore individual failures
        }
      });
    });
  }

  follow(conferenceId: string): void {
    const groupId = this.group()?.id;
    if (!groupId || this.isAlreadyFollowed(conferenceId)) return;

    const set = new Set(this.followingIds());
    set.add(conferenceId);
    this.followingIds.set(set);

    this.store.followConference(groupId, conferenceId);

    // Optimistically disable; store will refresh followed list on success
    setTimeout(() => {
      const s = new Set(this.followingIds());
      s.delete(conferenceId);
      this.followingIds.set(s);
    }, 1000);
  }

  isFollowing(conferenceId: string): boolean {
    return this.followingIds().has(conferenceId);
  }
}
