type RuntimeConfig = {
    apiUrl?: string;
};

declare global {
    interface Window {
        __ATTENDR_RUNTIME_CONFIG__?: RuntimeConfig;
    }
}

function getRuntimeConfig(): RuntimeConfig {
    if (typeof window === 'undefined') {
        return {};
    }

    return window.__ATTENDR_RUNTIME_CONFIG__ ?? {};
}

export function getRuntimeApiUrl(fallback: string): string {
    return getRuntimeConfig().apiUrl?.trim() || fallback;
}
