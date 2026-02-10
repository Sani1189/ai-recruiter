import { Configuration } from '@azure/msal-browser';
import { env } from './config/env';
// Candidate authentication - Azure B2C (public registration)
export const candidateMsalConfig: Configuration = {
   auth: {
    clientId: env.b2c.clientId,
    authority: env.b2c.authority,
    knownAuthorities: ['osilionrecruitment.ciamlogin.com'],
    redirectUri: typeof window !== 'undefined' ? window.location.origin : 'http://localhost:3000',
    postLogoutRedirectUri: typeof window !== 'undefined' ? window.location.origin : 'http://localhost:3000',
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: false,
    secureCookies: true,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) return;
        // switch (level) {
        //   case 0: console.error(message); break;
        //   case 1: console.warn(message); break;
        //   case 2: console.info(message); break;
        //   case 3: console.debug(message); break;
        // }
      },
    },
  },
};

// Login request for candidate authentication (CIAM)
export const candidateLoginRequest = {
  scopes: [
    'openid',
    'profile',
    'email',
    ...(env.b2c.apiScope ? [env.b2c.apiScope] : [])
  ],
  prompt: 'select_account',
};

