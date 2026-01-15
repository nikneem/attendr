import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

// Custom PrimeNG preset tuned to the Attendr dark palette defined in styles/_variables.scss
// Primary: Steel Blue (#4A90E2) - Secondary: Turquoise (#06b6d4)
export const AttendrPreset = definePreset(Aura, {
    semantic: {
        // Primary color palette - Steel Blue (from $primary-color in _variables.scss)
        primary: {
            50: '#e8f3fc',   // lightest
            100: '#c4e0f8',
            200: '#8cb6ed',  // $primary-lighter
            300: '#6ba3e8',  // $primary-light
            400: '#5a9ae5',
            500: '#4a90e2',  // $primary-color (base)
            600: '#3575c6',  // $primary-dark
            700: '#2d62a8',
            800: '#2a5da0',  // $primary-darker
            900: '#214a82',
            950: '#1a3b68'   // darkest
        },
        // Secondary color palette - Turquoise (from $secondary-color in _variables.scss)
        secondary: {
            50: '#ecfeff',   // lightest
            100: '#cffafe',
            200: '#a5f3fc',
            300: '#67e8f9',  // $secondary-lighter
            400: '#22d3ee',  // $secondary-light
            500: '#06b6d4',  // $secondary-color (base)
            600: '#0891b2',  // $secondary-dark
            700: '#0e7490',  // $secondary-darker
            800: '#155e75',
            900: '#164e63',
            950: '#083344'   // darkest
        },
        focusRing: {
            color: '#4a90e2'
        },
        colorScheme: {
            // Use the light slot as the default scheme; values align with the dark UI palette
            light: {
                surface: {
                    0: '#222222',
                    50: '#2a2a2a',
                    100: '#2a2a2a',
                    200: '#333333',
                    300: '#3a3a3a',
                    400: '#404040',
                    500: '#444444',
                    600: '#555555',
                    700: '#666666',
                    800: '#1f1f1f',
                    900: '#1a1a1a',
                    950: '#111111'
                },
                primary: {
                    color: '#4a90e2',        // $primary-color
                    contrastColor: '#222222', // $background-primary
                    hoverColor: '#6ba3e8',   // $primary-light
                    activeColor: '#3575c6'   // $primary-dark
                },
                highlight: {
                    background: '#0e7490',   // $secondary-darker
                    focusBackground: '#0891b2', // $secondary-dark
                    color: '#dddddd',        // $text-primary
                    focusColor: '#dddddd'    // $text-primary
                },
                mask: {
                    background: 'rgba(0, 0, 0, 0.7)',
                    color: '#3a3a3a'
                },
                formField: {
                    background: '#2a2a2a',
                    disabledBackground: '#333333',
                    filledBackground: '#2a2a2a',
                    filledHoverBackground: '#333333',
                    filledFocusBackground: '#333333',
                    borderColor: '#444444',
                    hoverBorderColor: '#555555',
                    focusBorderColor: '#4a90e2',
                    invalidBorderColor: '#ef4444',
                    color: '#dddddd',
                    disabledColor: '#666666',
                    placeholderColor: '#888888',
                    invalidPlaceholderColor: '#ef4444',
                    floatLabelColor: '#888888',
                    floatLabelFocusColor: '#4a90e2',
                    floatLabelActiveColor: '#bbbbbb',
                    floatLabelInvalidColor: '#ef4444',
                    iconColor: '#bbbbbb',
                    shadow: 'none'
                },
                text: {
                    color: '#dddddd',
                    hoverColor: '#ffffff',
                    mutedColor: '#888888',
                    hoverMutedColor: '#bbbbbb'
                },
                content: {
                    background: '#2a2a2a',
                    hoverBackground: '#333333',
                    borderColor: '#444444',
                    color: '#dddddd',
                    hoverColor: '#ffffff'
                },
                overlay: {
                    select: {
                        background: '#2a2a2a',
                        borderColor: '#444444',
                        color: '#dddddd'
                    },
                    popover: {
                        background: '#2a2a2a',
                        borderColor: '#444444',
                        color: '#dddddd'
                    },
                    modal: {
                        background: '#2a2a2a',
                        borderColor: '#444444',
                        color: '#dddddd'
                    }
                },
                list: {
                    option: {
                        focusBackground: '#333333',
                        selectedBackground: '#0e7490',
                        selectedFocusBackground: '#0891b2',
                        color: '#dddddd',
                        focusColor: '#ffffff',
                        selectedColor: '#dddddd',
                        selectedFocusColor: '#ffffff',
                        icon: {
                            color: '#888888',
                            focusColor: '#bbbbbb'
                        }
                    },
                    optionGroup: {
                        background: 'transparent',
                        color: '#888888'
                    }
                },
                navigation: {
                    item: {
                        focusBackground: '#333333',
                        activeBackground: '#333333',
                        color: '#dddddd',
                        focusColor: '#ffffff',
                        activeColor: '#ffffff',
                        icon: {
                            color: '#888888',
                            focusColor: '#bbbbbb',
                            activeColor: '#bbbbbb'
                        }
                    },
                    submenuLabel: {
                        background: 'transparent',
                        color: '#888888'
                    },
                    submenuIcon: {
                        color: '#888888',
                        focusColor: '#bbbbbb',
                        activeColor: '#bbbbbb'
                    }
                }
            }
        }
    },
    components: {
        // Customize badges to use accent colors for different severities
        badge: {
            colorScheme: {
                light: {
                    info: {
                        background: '#06b6d4',       // $secondary-color
                        color: '#ffffff'
                    },
                    success: {
                        background: '#10b981',       // $accent-success
                        color: '#ffffff'
                    },
                    warn: {
                        background: '#f59e0b',       // $accent-warning
                        color: '#222222'             // $text-inverse
                    },
                    danger: {
                        background: '#ef4444',       // $accent-error
                        color: '#ffffff'
                    }
                }
            }
        },
        // Customize messages to use accent colors
        message: {
            colorScheme: {
                light: {
                    info: {
                        background: 'rgba(6, 182, 212, 0.16)',  // $secondary-color with opacity
                        borderColor: '#06b6d4',
                        color: '#67e8f9',            // $secondary-lighter
                        shadow: '0px 4px 8px 0px rgba(6, 182, 212, 0.04)'
                    },
                    success: {
                        background: 'rgba(16, 185, 129, 0.16)', // $accent-success with opacity
                        borderColor: '#10b981',
                        color: '#34d399',
                        shadow: '0px 4px 8px 0px rgba(16, 185, 129, 0.04)'
                    },
                    warn: {
                        background: 'rgba(245, 158, 11, 0.16)', // $accent-warning with opacity
                        borderColor: '#f59e0b',
                        color: '#fbbf24',
                        shadow: '0px 4px 8px 0px rgba(245, 158, 11, 0.04)'
                    },
                    error: {
                        background: 'rgba(239, 68, 68, 0.16)',  // $accent-error with opacity
                        borderColor: '#ef4444',
                        color: '#f87171',
                        shadow: '0px 4px 8px 0px rgba(239, 68, 68, 0.04)'
                    }
                }
            }
        }
    }
});
