import { Configuration } from '@azure/msal-browser';
import { env } from './config/env';

// Recruiter authentication - Single-tenant Azure AD (admin only)
export const recruiterMsalConfig: Configuration = {
   auth: {
    clientId: env.azureAd.clientId,
    authority: env.azureAd.authority,
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

// Login request for recruiter authentication (Single-tenant AD)
export const recruiterLoginRequest = {
  scopes: ['openid', 'profile', 'email', env.azureAd.scope],
  prompt: 'select_account',
};


