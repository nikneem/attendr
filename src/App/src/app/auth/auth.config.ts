import { PassedInitialConfig } from 'angular-auth-oidc-client';
import { environment } from '../../environments/environment';

export const authConfig: PassedInitialConfig = {
  config: {
    authority: 'https://attendr.eu.auth0.com',
    redirectUrl: window.location.origin,
    clientId: 'uGNx931vqgAbGRXf0pDN6ApR5PDot9e8',
    scope: 'openid profile offline_access email https://api.attendr.com',
    responseType: 'code',
    silentRenew: true,
    useRefreshToken: true,
    // Disable automatic userInfo call which may interfere with token handling
    autoUserInfo: false,
    // Specify the audience to ensure Auth0 issues an access token instead of an opaque token
    customParamsAuthRequest: {
      audience: 'https://api.attendr.com'
    },
    // Configure secure routes - the library's authInterceptor will automatically add access tokens to these routes
    secureRoutes: [environment.apiUrl],
    // Default route after login when there is no prior target
    postLoginRoute: '/app/dashboard',
  }
}
