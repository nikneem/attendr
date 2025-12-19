import { PassedInitialConfig } from 'angular-auth-oidc-client';

export const authConfig: PassedInitialConfig = {
  config: {
    authority: 'https://attendr.eu.auth0.com',
    redirectUrl: window.location.origin,
    clientId: 'uGNx931vqgAbGRXf0pDN6ApR5PDot9e8',
    scope: 'openid profile offline_access https://api.attendr.com',
    responseType: 'code',
    silentRenew: true,
    useRefreshToken: true,
    // Default route after login when there is no prior target
    postLoginRoute: '/app/dashboard',
  }
}
