import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

// Custom PrimeNG preset tuned to the Attendr dark palette defined in styles/_variables.scss
export const AttendrPreset = definePreset(Aura, {
    semantic: {
        primary: {
            50: '#8cb6ed',
            100: '#6ba3e8',
            200: '#4a90e2',
            300: '#3575c6',
            400: '#2a5da0',
            500: '#4a90e2',
            600: '#3575c6',
            700: '#2a5da0',
            800: '#214a82',
            900: '#1a3b68',
            950: '#132b4d'
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
                    color: '#4a90e2',
                    contrastColor: '#222222',
                    hoverColor: '#6ba3e8',
                    activeColor: '#3575c6'
                },
                highlight: {
                    background: '#0e7490',
                    focusBackground: '#0891b2',
                    color: '#dddddd',
                    focusColor: '#dddddd'
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
    }
});
