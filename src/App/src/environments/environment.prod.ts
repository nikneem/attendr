import { getRuntimeApiUrl } from './runtime-config';

export const environment = {
    production: true,
    get apiUrl() {
        return getRuntimeApiUrl('https://gateway.thankfulhill-bb0bd872.northeurope.azurecontainerapps.io');
    },
    vapidPublicKey: 'BOnkb8qOr8a67pdpeQfvLYaOLsc-IA76OcNt1l030-NkosKALvAjNXOxrpetAKp0L_CCU0_fQ7Spk6CMZgHL57k',
    version: '{{VERSION}}',
};
