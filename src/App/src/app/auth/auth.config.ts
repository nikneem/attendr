import { PassedInitialConfig } from 'angular-auth-oidc-client';

export const authConfig: PassedInitialConfig = {
  config: {
    authority: 'attendr.eu.auth0.com',
    redirectUrl: window.location.origin,
    clientId: 'uGNx931vqgAbGRXf0pDN6ApR5PDot9e8',
    scope: 'openid profile offline_access',
    responseType: 'code',
    silentRenew: true,
    useRefreshToken: true,
  }
}
