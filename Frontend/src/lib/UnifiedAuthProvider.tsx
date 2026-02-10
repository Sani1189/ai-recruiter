'use client';

import { PublicClientApplication } from '@azure/msal-browser';
import { MsalProvider } from '@azure/msal-react';
import { ReactNode, createContext, useContext } from 'react';

import { candidateMsalConfig } from './candidateAuthConfig';
import { recruiterMsalConfig } from './recruiterAuthConfig';

// Create separate MSAL instances for different tenant types
let candidateMsalInstance: PublicClientApplication | null = null;
let recruiterMsalInstance: PublicClientApplication | null = null;

// Initialize instances only once
const getCandidateMsalInstance = () => {
  if (!candidateMsalInstance) {
    candidateMsalInstance = new PublicClientApplication(candidateMsalConfig);
    candidateMsalInstance.initialize();
  }
  return candidateMsalInstance;
};

const getRecruiterMsalInstance = () => {
  if (!recruiterMsalInstance) {
    recruiterMsalInstance = new PublicClientApplication(recruiterMsalConfig);
    recruiterMsalInstance.initialize();
  }
  return recruiterMsalInstance;
};

// Create context for recruiter instance
const RecruiterMsalContext = createContext<PublicClientApplication | null>(null);

interface UnifiedAuthProviderProps {
  children: ReactNode;
}

export function UnifiedAuthProvider({ children }: UnifiedAuthProviderProps) {
  const candidateInstance = getCandidateMsalInstance();
  const recruiterInstance = getRecruiterMsalInstance();
  
  return (
    <MsalProvider instance={candidateInstance}>
      <RecruiterMsalContext.Provider value={recruiterInstance}>
        {children}
      </RecruiterMsalContext.Provider>
    </MsalProvider>
  );
}

// Hook to get recruiter instance
export function useRecruiterMsal() {
  const context = useContext(RecruiterMsalContext);
  if (!context) {
    throw new Error('useRecruiterMsal must be used within UnifiedAuthProvider');
  }
  return context;
}

// Export getter functions for direct use
export { getCandidateMsalInstance, getRecruiterMsalInstance };
