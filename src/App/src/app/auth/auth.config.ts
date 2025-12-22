import { PassedInitialConfig } from 'angular-auth-oidc-client';
import { environment } from '../../environments/environment';

export const authConfig: PassedInitialConfig = {
  config: {
    authority: 'https://attendr.eu.auth0.com',
    redirectUrl: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
    clientId: 'uGNx931vqgAbGRXf0pDN6ApR5PDot9e8',
    scope: 'openid profile offline_access email',
    responseType: 'code',
    silentRenew: true,
    useRefreshToken: true,
    renewTimeBeforeTokenExpiresInSeconds: 30,
    // Disable automatic userInfo call which may interfere with token handling
    autoUserInfo: false,
    // Auth0 specific configuration
    authWellknownEndpointUrl: 'https://attendr.eu.auth0.com/.well-known/openid-configuration',
    // Specify the audience to ensure Auth0 issues an access token instead of an opaque token
    customParamsAuthRequest: {
      audience: 'https://api.attendr.com'
    },
    // Configure secure routes - the library's authInterceptor will automatically add access tokens to these routes
    secureRoutes: [environment.apiUrl],
    // Default route after login when there is no prior target
    postLoginRoute: '/app/dashboard',
    // Auth0 requires this to be set to avoid CORS issues
    ignoreNonceAfterRefresh: true,
  }
}
