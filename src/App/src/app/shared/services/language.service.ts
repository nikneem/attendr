import { Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

export const DEFAULT_LANG = 'en' as const;
export const SUPPORTED_LANGS = ['en', 'nl'] as const;
export type SupportedLang = (typeof SUPPORTED_LANGS)[number];
const STORAGE_KEY = 'attendr.language';

export interface SupportedLanguage {
  code: SupportedLang;
  label: string;
}

export const SUPPORTED_LANGUAGE_LIST: SupportedLanguage[] = [
  { code: 'en', label: 'English' },
  { code: 'nl', label: 'Dutch' },
];

@Injectable({
  providedIn: 'root',
})
export class LanguageService {
  private readonly translate = inject(TranslateService);

  readonly currentLang = signal<SupportedLang>(DEFAULT_LANG);

  constructor() {
    this.translate.onLangChange.subscribe(({ lang }) => {
      const validLang = this.isSupported(lang) ? lang : DEFAULT_LANG;
      this.currentLang.set(validLang);
      document.documentElement.lang = validLang;
    });
  }

  resolveBrowserLanguage(): SupportedLang | null {
    const langs: readonly string[] = navigator.languages?.length
      ? navigator.languages
      : [this.translate.getBrowserLang() ?? ''];

    for (const raw of langs) {
      const base = raw.toLowerCase().split('-')[0] as SupportedLang;
      if (this.isSupported(base)) {
        return base;
      }
    }
    return null;
  }

  resolvePublicLanguage(): { lang: SupportedLang; source: 'browser' | 'fallback' } {
    const browser = this.resolveBrowserLanguage();
    if (browser) {
      return { lang: browser, source: 'browser' };
    }
    return { lang: DEFAULT_LANG, source: 'fallback' };
  }

  resolveAuthenticatedLanguage(): { lang: SupportedLang; source: 'stored' | 'browser' | 'fallback' } {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored && this.isSupported(stored)) {
      return { lang: stored as SupportedLang, source: 'stored' };
    }
    const browser = this.resolveBrowserLanguage();
    if (browser) {
      return { lang: browser, source: 'browser' };
    }
    return { lang: DEFAULT_LANG, source: 'fallback' };
  }

  async applyPublicLanguage(): Promise<void> {
    this.translate.setDefaultLang(DEFAULT_LANG);
    const { lang, source } = this.resolvePublicLanguage();
    console.debug(`Language resolved: ${lang} (context: public, source: ${source})`);
    await this.translate.use(lang).toPromise();
  }

  async applyAuthenticatedLanguage(): Promise<void> {
    this.translate.setDefaultLang(DEFAULT_LANG);
    const { lang, source } = this.resolveAuthenticatedLanguage();
    console.debug(`Language resolved: ${lang} (context: authenticated, source: ${source})`);
    localStorage.setItem(STORAGE_KEY, lang);
    await this.translate.use(lang).toPromise();
  }

  async setLanguage(lang: string): Promise<void> {
    if (!this.isSupported(lang)) {
      console.warn(`Language "${lang}" is not supported. Supported: ${SUPPORTED_LANGS.join(', ')}`);
      return;
    }
    localStorage.setItem(STORAGE_KEY, lang);
    await this.translate.use(lang).toPromise();
  }

  private isSupported(lang: string): lang is SupportedLang {
    return (SUPPORTED_LANGS as readonly string[]).includes(lang);
  }
}
