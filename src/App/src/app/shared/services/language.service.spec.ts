import { TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';
import { TranslateService, LangChangeEvent } from '@ngx-translate/core';
import { LanguageService, DEFAULT_LANG, SUPPORTED_LANGS } from './language.service';

const STORAGE_KEY = 'attendr.language';

function makeMockTranslate(overrides: Partial<TranslateService> = {}): TranslateService {
  const langChanges = new Subject<LangChangeEvent>();
  return {
    setDefaultLang: () => {},
    use: (lang: string) => {
      langChanges.next({ lang, translations: {} } as LangChangeEvent);
      return new Subject<any>().asObservable();
    },
    getBrowserLang: () => 'en',
    onLangChange: langChanges,
    ...overrides,
  } as unknown as TranslateService;
}

describe('LanguageService', () => {
  let service: LanguageService;
  let mockTranslate: TranslateService;

  const setupService = (translateOverrides: Partial<TranslateService> = {}) => {
    mockTranslate = makeMockTranslate(translateOverrides);
    TestBed.configureTestingModule({
      providers: [
        LanguageService,
        { provide: TranslateService, useValue: mockTranslate },
      ],
    });
    service = TestBed.inject(LanguageService);
  };

  beforeEach(() => {
    localStorage.removeItem(STORAGE_KEY);
  });

  afterEach(() => {
    localStorage.removeItem(STORAGE_KEY);
    TestBed.resetTestingModule();
  });

  describe('resolveBrowserLanguage', () => {
    it('returns supported language matching browser preference', () => {
      setupService({ getBrowserLang: () => 'nl' } as any);
      Object.defineProperty(navigator, 'languages', {
        value: ['nl-NL', 'en-US'],
        configurable: true,
      });
      expect(service.resolveBrowserLanguage()).toBe('nl');
    });

    it('returns en when browser prefers en-US', () => {
      setupService();
      Object.defineProperty(navigator, 'languages', {
        value: ['en-US'],
        configurable: true,
      });
      expect(service.resolveBrowserLanguage()).toBe('en');
    });

    it('returns null when no browser language is supported', () => {
      setupService({ getBrowserLang: () => 'de' } as any);
      Object.defineProperty(navigator, 'languages', {
        value: ['de-DE', 'fr'],
        configurable: true,
      });
      expect(service.resolveBrowserLanguage()).toBeNull();
    });
  });

  describe('resolvePublicLanguage', () => {
    it('uses browser language when supported', () => {
      setupService();
      Object.defineProperty(navigator, 'languages', {
        value: ['nl'],
        configurable: true,
      });
      const result = service.resolvePublicLanguage();
      expect(result.lang).toBe('nl');
      expect(result.source).toBe('browser');
    });

    it('falls back to en when browser language is unsupported', () => {
      setupService({ getBrowserLang: () => 'de' } as any);
      Object.defineProperty(navigator, 'languages', {
        value: ['de'],
        configurable: true,
      });
      const result = service.resolvePublicLanguage();
      expect(result.lang).toBe(DEFAULT_LANG);
      expect(result.source).toBe('fallback');
    });

    it('does not write to localStorage', () => {
      setupService();
      Object.defineProperty(navigator, 'languages', { value: ['nl'], configurable: true });
      service.resolvePublicLanguage();
      expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
    });
  });

  describe('resolveAuthenticatedLanguage', () => {
    it('prefers stored language when valid', () => {
      setupService();
      localStorage.setItem(STORAGE_KEY, 'nl');
      const result = service.resolveAuthenticatedLanguage();
      expect(result.lang).toBe('nl');
      expect(result.source).toBe('stored');
    });

    it('ignores stored language when invalid and falls back to browser', () => {
      setupService({ getBrowserLang: () => 'en' } as any);
      localStorage.setItem(STORAGE_KEY, 'de');
      Object.defineProperty(navigator, 'languages', { value: ['en'], configurable: true });
      const result = service.resolveAuthenticatedLanguage();
      expect(result.lang).toBe('en');
      expect(result.source).toBe('browser');
    });

    it('falls back to en when no storage and no supported browser language', () => {
      setupService({ getBrowserLang: () => 'de' } as any);
      Object.defineProperty(navigator, 'languages', { value: ['de'], configurable: true });
      const result = service.resolveAuthenticatedLanguage();
      expect(result.lang).toBe(DEFAULT_LANG);
      expect(result.source).toBe('fallback');
    });
  });

  describe('applyPublicLanguage', () => {
    it('applies resolved browser language without writing to localStorage', async () => {
      const useSpy = vi.fn().mockReturnValue({ toPromise: () => Promise.resolve(null) });
      setupService({ use: useSpy, getBrowserLang: () => 'nl' } as any);
      Object.defineProperty(navigator, 'languages', { value: ['nl'], configurable: true });

      await service.applyPublicLanguage();

      expect(useSpy).toHaveBeenCalledWith('nl');
      expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
    });
  });

  describe('applyAuthenticatedLanguage', () => {
    it('uses stored language and writes it back to localStorage', async () => {
      const useSpy = vi.fn().mockReturnValue({ toPromise: () => Promise.resolve(null) });
      setupService({ use: useSpy } as any);
      localStorage.setItem(STORAGE_KEY, 'nl');

      await service.applyAuthenticatedLanguage();

      expect(useSpy).toHaveBeenCalledWith('nl');
      expect(localStorage.getItem(STORAGE_KEY)).toBe('nl');
    });

    it('writes browser language to localStorage when no stored language', async () => {
      const useSpy = vi.fn().mockReturnValue({ toPromise: () => Promise.resolve(null) });
      setupService({ use: useSpy, getBrowserLang: () => 'en' } as any);
      Object.defineProperty(navigator, 'languages', { value: ['en'], configurable: true });

      await service.applyAuthenticatedLanguage();

      expect(useSpy).toHaveBeenCalledWith('en');
      expect(localStorage.getItem(STORAGE_KEY)).toBe('en');
    });
  });

  describe('setLanguage', () => {
    it('validates supported language and persists to localStorage', async () => {
      const useSpy = vi.fn().mockReturnValue({ toPromise: () => Promise.resolve(null) });
      setupService({ use: useSpy } as any);

      await service.setLanguage('nl');

      expect(useSpy).toHaveBeenCalledWith('nl');
      expect(localStorage.getItem(STORAGE_KEY)).toBe('nl');
    });

    it('rejects unsupported languages and does not write to localStorage', async () => {
      const useSpy = vi.fn().mockReturnValue({ toPromise: () => Promise.resolve(null) });
      setupService({ use: useSpy } as any);

      await service.setLanguage('de');

      expect(useSpy).not.toHaveBeenCalled();
      expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
    });

    it('accepts all supported languages', async () => {
      const useSpy = vi.fn().mockReturnValue({ toPromise: () => Promise.resolve(null) });
      setupService({ use: useSpy } as any);

      for (const lang of SUPPORTED_LANGS) {
        localStorage.removeItem(STORAGE_KEY);
        await service.setLanguage(lang);
        expect(localStorage.getItem(STORAGE_KEY)).toBe(lang);
      }
    });
  });
});
