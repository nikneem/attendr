# Attendr Angular Frontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.0.0.

## Important: Zoneless Configuration

**This application is configured to run without Zone.js (zoneless mode).** This is a modern Angular configuration that provides better performance and more explicit change detection.

### Key Requirements for Zoneless Angular

When developing components and features for this application, you must follow these practices:

1. **Use Signals for Reactive State**
   - All component state that needs to trigger UI updates must use Angular signals
   - Use `signal()` for mutable state, `computed()` for derived state
   - Example:
     ```typescript
     import { Component, signal } from '@angular/core';
     
     export class MyComponent {
       loading = signal(false);
       data = signal<MyData[]>([]);
       count = computed(() => this.data().length);
     }
     ```

2. **Template Signal Syntax**
   - Always call signals in templates using parentheses: `loading()`, `data()`
   - Example:
     ```html
     <div *ngIf="loading()">Loading...</div>
     <div *ngFor="let item of data()">{{ item.name }}</div>
     ```

3. **Update Signals with `.set()` or `.update()`**
   - Use `.set(value)` to replace the entire value
   - Use `.update(fn)` to modify based on the current value
   - Example:
     ```typescript
     this.loading.set(true);
     this.data.set(newData);
     this.count.update(c => c + 1);
     ```

4. **Avoid Direct Property Mutations**
   - ❌ Wrong: `this.loading = false;` (will not trigger change detection)
   - ✅ Correct: `this.loading.set(false);` (triggers change detection)

5. **No Zone.js Workarounds**
   - Do not use `setTimeout()`, `ChangeDetectorRef.detectChanges()`, or `markForCheck()`
   - These are Zone.js patterns and are not needed with signals

### Migration Pattern

If you encounter components using old patterns, migrate them to signals:

**Before (Zone.js style):**
```typescript
export class OldComponent {
  loading = false;
  data: MyData[] = [];
  
  loadData() {
    this.loading = true;
    this.service.getData().subscribe(result => {
      this.data = result;
      this.loading = false;
    });
  }
}
```

**After (Zoneless style):**
```typescript
export class NewComponent {
  loading = signal(false);
  data = signal<MyData[]>([]);
  
  loadData() {
    this.loading.set(true);
    this.service.getData().subscribe(result => {
      this.data.set(result);
      this.loading.set(false);
    });
  }
}
```

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Karma](https://karma-runner.github.io) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
