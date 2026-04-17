import { getRuntimeApiUrl } from './runtime-config';

export const environment = {
    production: false,
    get apiUrl() {
        return getRuntimeApiUrl('http://localhost:5000');
    },
    vapidPublicKey: 'BE76qGxLqY2Wue_eikccYSsLqUrH9_2oUkSM2qddNetetHxCgTdGVB9R1u1fYSSr9iVjmXfTjrZmAiRUcm1MgAc',
    version: 'dev-local',
};
