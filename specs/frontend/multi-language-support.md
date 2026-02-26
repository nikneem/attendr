# Spec: Multi-Language Support (i18n)

## Summary
Add runtime multi-language support to the Attendr Angular 21 frontend using **@ngx-translate/core** and **@ngx-translate/http-loader**.

- Default + fallback language: **English** (`en`).
- Supported languages are **static in code** and for now only: **English (`en`)** and **Dutch (`nl`)**.
- Language selection is context-dependent:
  - **Public pages**: always use the browser language (first supported match), otherwise fall back to `en`.
  - **Authenticated pages**: use the stored language from `localStorage` if present (leading). Otherwise use the browser language (first supported match), otherwise fall back to `en`. Persist the chosen language to `localStorage`.
- Users can manually switch language on the **Account preferences** page via a dropdown. This setting is persisted and takes precedence on return visits (authenticated pages).

## Context
- The app is Angular 21, standalone components, and zoneless (signal-based state).
- No i18n library is currently installed.
- Most user-facing strings are hard-coded in templates and TypeScript.
- Translation files must be served from `public/`.
- Documentation:
  - https://ngx-translate.org/
  - https://ngx-translate.org/reference/translate-service-api/
  - https://ngx-translate.org/reference/translate-loader-api/

## Goals
- Install `@ngx-translate/core` and `@ngx-translate/http-loader`.
- Configure `TranslateHttpLoader` to load `public/translations/{lang}.json`.
- Provide a `LanguageService` that:
  - Resolves the active language according to the rules above.
  - Stores the user-selected language in `localStorage` and treats it as leading (authenticated pages).
  - Exposes a signal for the active language and supported-language metadata for the UI.
- Add a language dropdown to the Account preferences page.
- Ensure `<html lang>` reflects the active language.

## Non-goals
- Translating all existing strings (follow-up work per feature area).
- RTL support.
- Backend language negotiation.
- ICU message format support.

## Supported languages
Static list (code-driven):

| Code | Language |
|------|----------|
| `en` | English (default + fallback) |
| `nl` | Dutch |

## UX / behavior

### Public pages
- On app init (when unauthenticated), pick the language based on browser preferences.
- Do not allow changing language from public pages.
- Do not persist the language for public pages.

### Authenticated pages
- Once authenticated, apply the leading language:
  1. If `localStorage` contains a valid, supported language → use it.
  2. Else resolve using browser language (first supported match) → else `en`.
  3. Persist the chosen language to `localStorage`.

### Manual language switching
- The user can change language from the **Account preferences** page using a dropdown.
- Changing language updates translations immediately (no reload) and persists to `localStorage`.

## Language resolution algorithm

### Browser languages
Use the browser-preferred languages list and pick the first supported match:
- Primary: `navigator.languages` (ordered list).
- Fallback: `TranslateService.getBrowserLang()`.

Normalize each entry by:
- Lowercasing
- Taking the base language subtag: e.g. `"nl-NL" -> "nl"`, `"en-US" -> "en"`

### Public resolution
```
lang = firstSupported(browserLanguages) ?? 'en'
```

### Authenticated resolution
```
if (localStorage has supported value) use it
else lang = firstSupported(browserLanguages) ?? 'en'
store lang in localStorage
```

## Architecture

### Package installation
```
npm install @ngx-translate/core @ngx-translate/http-loader
```

### Translation files
Location: `src/App/public/translations/{language}.json`

Example:
```
public/
  translations/
    en.json
    nl.json
```

JSON can be nested; namespacing is encouraged:
```json
{
  "COMMON": {
    "SAVE": "Save",
    "CANCEL": "Cancel"
  },
  "PREFERENCES": {
    "LANGUAGE": "Language"
  }
}
```

### `LanguageService`
Location: `src/app/shared/services/language.service.ts`

Responsibilities:
- Constants:
  - `DEFAULT_LANG = 'en'`
  - `SUPPORTED_LANGS = ['en', 'nl'] as const`
  - `STORAGE_KEY = 'attendr.language'`
- `resolveBrowserLanguage(): 'en' | 'nl' | null`
- `resolvePublicLanguage(): { lang: string; source: 'browser' | 'fallback' }`
- `resolveAuthenticatedLanguage(): { lang: string; source: 'stored' | 'browser' | 'fallback' }`
- `applyPublicLanguage(): Promise<void>` → calls `translate.setFallbackLang('en')`, then `translate.use(resolvedLang)`
- `applyAuthenticatedLanguage(): Promise<void>` → same, but uses stored-leading logic and persists
- `setLanguage(lang: string): Promise<void>`:
  - validate `lang` is supported
  - write to `localStorage`
  - `translate.use(lang)`
- Track active language:
  - `currentLang = signal<string>('en')`
  - update on `translate.onLangChange`

### App initialization (`app.config.ts`)
1. Provide ngx-translate (standalone provider API) and Http loader:
```ts
import { provideTranslateService, TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { HttpClient } from '@angular/common/http';

export function createTranslateLoader(http: HttpClient) {
  return new TranslateHttpLoader(http, '/translations/', '.json');
}

// in providers:
provideTranslateService({
  loader: {
    provide: TranslateLoader,
    useFactory: createTranslateLoader,
    deps: [HttpClient],
  },
}),
```

2. Use `APP_INITIALIZER` to load translations before first render:
- If the app can determine auth status on init (e.g., via the existing OIDC service), call either:
  - `LanguageService.applyPublicLanguage()` when unauthenticated
  - `LanguageService.applyAuthenticatedLanguage()` when authenticated
- Additionally, on auth-state transitions (login/logout), re-apply the appropriate language (public vs authenticated).

### Language dropdown (Account preferences)
- Add a dropdown to the existing page:
  - `src/app/pages/private/preferences/account-preferences-page.component.*`
- The dropdown binds to `LanguageService.currentLang()` and calls `LanguageService.setLanguage()`.

### `<html lang>` sync
- When the language changes (`TranslateService.onLangChange`), set:
  - `document.documentElement.lang = currentLang`

## API / contracts

### Static asset URL
- `GET /translations/{lang}.json`

### `localStorage`
- Key: `attendr.language`
- Value: `en` or `nl`

## Observability
- Debug log on every apply:
  - `Language resolved: {lang} (context: public|authenticated, source: stored|browser|fallback)`

## Security
- Validate any stored language against `SUPPORTED_LANGS` before calling `translate.use()`.

## Acceptance criteria
- Supported languages are exactly `en` and `nl` (static list in code).
- Public pages:
  - If browser preferences include `nl` (or `nl-*`), the app uses `nl`.
  - If no supported browser language exists, the app uses `en`.
  - Public pages do not persist language.
- Authenticated pages:
  - If `localStorage[attendr.language]` exists and is valid, it is used.
  - Otherwise the first supported browser language is used; otherwise `en`.
  - The chosen language is stored in `localStorage`.
- The user can switch language from Account preferences via dropdown, and this choice is persisted.
- Switching language updates `| translate` pipe bindings without a reload.
- `<html lang>` is updated to the active language.

## Test plan
- Unit tests for `LanguageService`:
  - public resolution uses browser only and doesn’t write storage
  - authenticated resolution prefers storage when valid
  - authenticated resolution falls back to browser / `en` and writes storage
  - `setLanguage()` validates and persists
- Component test for the Account preferences dropdown:
  - selecting `nl` calls `setLanguage('nl')`
- Build: `npm run build`

## Rollout
- Infra-first: introduce loader/service/initialization and the language selector.
- Follow-up specs will convert hard-coded strings to translation keys.

## Open questions
- None.
