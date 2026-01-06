# Angular Development Guidelines

This document outlines the coding standards and best practices for developing Angular components in the Attendr application.

## Zoneless Angular Architecture

This application uses **Angular 21+ in zoneless mode** (without Zone.js). This architectural decision impacts how you write components.

### Why Zoneless?

- **Better Performance**: No Zone.js overhead for change detection
- **Explicit Control**: Developers explicitly control when UI updates occur
- **Modern Standards**: Aligns with Angular's future direction and signal-based reactivity
- **Smaller Bundle Size**: Eliminates Zone.js dependency (~30KB)

## Component Development Standards

### 1. State Management with Signals

All reactive state must use Angular signals:

```typescript
import { Component, signal, computed } from '@angular/core';

export class MyComponent {
  // ✅ Use signals for state
  loading = signal(false);
  data = signal<Item[]>([]);
  error = signal<string | null>(null);
  
  // ✅ Use computed for derived state
  itemCount = computed(() => this.data().length);
  hasData = computed(() => this.data().length > 0);
  
  // ❌ NEVER use plain properties for reactive state
  // wrongLoading = false; // This won't trigger change detection!
}
```

### 2. Template Syntax

Always use signal call syntax in templates:

```html
<!-- ✅ Correct: Call signals with parentheses -->
<div *ngIf="loading()">Loading...</div>
<div *ngIf="error()">{{ error() }}</div>
<div *ngFor="let item of data()">{{ item.name }}</div>

<!-- ❌ Wrong: Accessing signals without calling them -->
<div *ngIf="loading">Loading...</div>
```

### 3. Updating Signals

Use `.set()` and `.update()` methods:

```typescript
// ✅ Replace entire value with .set()
this.loading.set(true);
this.data.set(newData);
this.error.set('Failed to load data');

// ✅ Modify based on current value with .update()
this.count.update(current => current + 1);
this.items.update(current => [...current, newItem]);

// ❌ NEVER assign directly
// this.loading = true; // Will NOT work!
```

### 4. HTTP Requests and Async Operations

Handle async operations with signals:

```typescript
loadData(): void {
  this.loading.set(true);
  this.error.set(null);
  
  this.dataService.getData().subscribe({
    next: (result) => {
      this.data.set(result);
      this.loading.set(false);
    },
    error: (err) => {
      console.error('Error loading data:', err);
      this.error.set('Failed to load data. Please try again.');
      this.loading.set(false);
    }
  });
}
```

### 5. Component Initialization

Set initial signal states that match your UI requirements:

```typescript
export class MyComponent implements OnInit {
  // If you load data in ngOnInit, start with loading = true
  loading = signal(true);
  data = signal<Item[]>([]);
  
  ngOnInit(): void {
    this.loadData(); // Will update signals when complete
  }
}
```

## Common Patterns

### Loading States

```typescript
export class DataListComponent {
  loading = signal(true);
  data = signal<Item[]>([]);
  error = signal<string | null>(null);
  
  // Computed for empty state
  isEmpty = computed(() => !this.loading() && this.data().length === 0);
  hasError = computed(() => this.error() !== null);
}
```

Template:
```html
<div *ngIf="loading()">Loading...</div>
<div *ngIf="hasError()">{{ error() }}</div>
<div *ngIf="isEmpty()">No items found.</div>
<div *ngIf="!loading() && data().length > 0">
  <div *ngFor="let item of data()">{{ item.name }}</div>
</div>
```

### Forms and User Input

```typescript
export class FormComponent {
  formValue = signal('');
  isValid = computed(() => this.formValue().length > 0);
  
  onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.formValue.set(input.value);
  }
}
```

### Navigation

```typescript
import { Router } from '@angular/router';

export class ListComponent {
  private readonly router = inject(Router);
  
  navigateToDetail(id: string): void {
    this.router.navigate(['/app/items', id]);
  }
}
```

## Anti-Patterns to Avoid

### ❌ Don't Use Zone.js Patterns

```typescript
// ❌ NO setTimeout for change detection
setTimeout(() => {
  this.loading.set(false);
});

// ❌ NO ChangeDetectorRef
constructor(private cdr: ChangeDetectorRef) {}
this.cdr.detectChanges();
this.cdr.markForCheck();

// ✅ Just use signals directly
this.loading.set(false); // This is enough!
```

### ❌ Don't Mix Signals and Plain Properties

```typescript
// ❌ Bad: Mixing patterns
export class BadComponent {
  loading = signal(true);     // Signal
  data: Item[] = [];          // Plain property - won't work!
}

// ✅ Good: All reactive state as signals
export class GoodComponent {
  loading = signal(true);     // Signal
  data = signal<Item[]>([]);  // Signal
}
```

### ❌ Don't Forget to Call Signals in Templates

```typescript
// Component
export class MyComponent {
  visible = signal(false);
}

// ❌ Wrong template
<div *ngIf="visible">Content</div>

// ✅ Correct template
<div *ngIf="visible()">Content</div>
```

## Testing Signals

When testing components with signals:

```typescript
describe('MyComponent', () => {
  it('should update loading state', () => {
    const component = new MyComponent();
    
    // Read signal value with ()
    expect(component.loading()).toBe(true);
    
    // Update signal with .set()
    component.loading.set(false);
    
    // Verify update
    expect(component.loading()).toBe(false);
  });
  
  it('should compute derived values', () => {
    const component = new MyComponent();
    component.data.set([item1, item2, item3]);
    
    // Computed values update automatically
    expect(component.itemCount()).toBe(3);
  });
});
```

## Component Structure

Follow this structure for consistency:

```typescript
import { Component, inject, signal, computed, OnInit } from '@angular/core';

@Component({
  selector: 'app-my-component',
  standalone: true,
  imports: [CommonModule, /* other modules */],
  templateUrl: './my-component.component.html',
  styleUrl: './my-component.component.scss',
})
export class MyComponent implements OnInit {
  // 1. Services (injected)
  private readonly myService = inject(MyService);
  private readonly router = inject(Router);
  
  // 2. Signals (reactive state)
  loading = signal(false);
  data = signal<Item[]>([]);
  error = signal<string | null>(null);
  
  // 3. Computed signals (derived state)
  isEmpty = computed(() => this.data().length === 0);
  
  // 4. Lifecycle hooks
  ngOnInit(): void {
    this.loadData();
  }
  
  // 5. Public methods
  loadData(): void {
    // Implementation
  }
  
  // 6. Event handlers
  onItemClick(id: string): void {
    // Implementation
  }
  
  // 7. Helper methods
  private formatData(data: RawData): Item[] {
    // Implementation
  }
}
```

## Further Reading

- [Angular Signals Documentation](https://angular.dev/guide/signals)
- [Zoneless Change Detection](https://angular.dev/guide/experimental/zoneless)
- [Angular 21 Features](https://angular.dev)

## Questions?

If you're unsure about how to implement something in zoneless mode, check existing components like:
- `my-conferences.component.ts` - Example of signals with HTTP requests
- `joined-groups.component.ts` - Example with navigation
- Stores in `/stores` folder - Examples of signal-based state management
