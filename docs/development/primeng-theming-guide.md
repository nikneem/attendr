# PrimeNG Theming Guide for Attendr

This guide explains how the Attendr application is styled using PrimeNG's design token system.

## Overview

Attendr uses a custom PrimeNG preset that implements the application's dark theme with the primary and secondary colors defined in `_variables.scss`. The theming system follows PrimeNG's best practices by using design tokens instead of CSS overrides.

## Color Palette

### Primary Color - Steel Blue
- Base: `#4A90E2` (from `$primary-color` in _variables.scss)
- Light: `#6BA3E8` (from `$primary-light`)
- Lighter: `#8CB6ED` (from `$primary-lighter`)
- Dark: `#3575C6` (from `$primary-dark`)
- Darker: `#2A5DA0` (from `$primary-darker`)

The primary color is used for:
- Default button backgrounds
- Focus rings and borders
- Primary UI elements and links

### Secondary Color - Turquoise
- Base: `#06B6D4` (from `$secondary-color` in _variables.scss)
- Light: `#22D3EE` (from `$secondary-light`)
- Lighter: `#67E8F9` (from `$secondary-lighter`)
- Dark: `#0891B2` (from `$secondary-dark`)
- Darker: `#0E7490` (from `$secondary-darker`)

The secondary color is used for:
- Highlights and selections
- Secondary button variants
- Info badges and messages

### Accent Colors
- **Success**: `#10B981` (from `$accent-success`)
- **Warning**: `#F59E0B` (from `$accent-warning`)
- **Error**: `#EF4444` (from `$accent-error`)
- **Info**: `#3B82F6` (from `$accent-info`)

## Configuration

### Theme Preset Location
The custom preset is defined in: [`app/theme/attendr.preset.ts`](../../src/App/src/app/theme/attendr.preset.ts)

### How It's Applied
The preset is configured in [`app/app.config.ts`](../../src/App/src/app/app.config.ts):

```typescript
providePrimeNG({
  theme: {
    preset: AttendrPreset
  }
})
```

## Using Colors in Components

### Buttons

#### Default Button (Primary Color)
```html
<p-button label="Primary Action" />
```

#### Secondary Button (Turquoise)
```html
<p-button label="Secondary Action" severity="secondary" />
```

#### Other Severities
```html
<p-button label="Success" severity="success" />
<p-button label="Info" severity="info" />
<p-button label="Warning" severity="warn" />
<p-button label="Danger" severity="danger" />
```

### Badges

```html
<p-badge value="2" severity="info" />      <!-- Turquoise -->
<p-badge value="5" severity="success" />   <!-- Green -->
<p-badge value="!" severity="warn" />      <!-- Amber -->
<p-badge value="9" severity="danger" />    <!-- Red -->
```

### Messages

```html
<p-message severity="info" text="Information message" />    <!-- Turquoise theme -->
<p-message severity="success" text="Success message" />     <!-- Green theme -->
<p-message severity="warn" text="Warning message" />        <!-- Amber theme -->
<p-message severity="error" text="Error message" />         <!-- Red theme -->
```

## Design Token Structure

The preset follows PrimeNG's three-tier design token structure:

### 1. Primitive Tokens
Raw color values mapped from `_variables.scss`:
- `primary.{50-950}` - Steel Blue palette
- `secondary.{50-950}` - Turquoise palette

### 2. Semantic Tokens
Context-aware tokens that map to primitives:
- `primary.color` - Main brand color
- `highlight.background` - Selection backgrounds
- `surface.*` - Background colors for dark theme
- `text.color` - Text colors
- `formField.*` - Form input styles

### 3. Component Tokens
Component-specific customizations:
- `button.*` - Button color schemes
- `badge.*` - Badge variants
- `message.*` - Message component styling

## Best Practices

### ✅ DO
- Use severity attributes to apply color variants
- Reference design tokens when extending styles
- Update `attendr.preset.ts` for global theme changes
- Keep `_variables.scss` and `attendr.preset.ts` in sync

### ❌ DON'T
- Use `::ng-deep` to override component colors with `!important`
- Hard-code color values in component templates
- Create custom CSS classes for color variants
- Override PrimeNG styles directly in component stylesheets

## Customization

### Updating the Theme

To modify the theme globally, edit [`attendr.preset.ts`](../../src/App/src/app/theme/attendr.preset.ts):

```typescript
const MyCustomPreset = definePreset(Aura, {
  semantic: {
    primary: {
      // Update primary color palette
    },
    colorScheme: {
      light: {
        // Update component tokens
      }
    }
  },
  components: {
    button: {
      // Customize specific components
    }
  }
});
```

### Scoped Component Customization

For component-specific overrides, use the `dt` property:

```typescript
@Component({
  template: `<p-button [dt]="customButtonStyle" />`
})
export class MyComponent {
  customButtonStyle = {
    colorScheme: {
      light: {
        root: {
          background: '#custom-color'
        }
      }
    }
  };
}
```

## Color Scheme

The application uses a **dark theme** as its default (and currently only) color scheme. The `colorScheme.light` section in the preset actually contains the dark theme values, as the application doesn't have a light mode toggle.

If you need to add a proper light/dark mode toggle in the future:
1. Add dark mode values in `colorScheme.dark`
2. Configure `darkModeSelector` in app.config.ts
3. Implement a toggle component

## References

- [PrimeNG Theming Documentation](https://primeng.org/theming/styled)
- [Variables File](../../src/App/src/styles/_variables.scss)
- [Attendr Preset](../../src/App/src/app/theme/attendr.preset.ts)
- [Application Config](../../src/App/src/app/app.config.ts)
