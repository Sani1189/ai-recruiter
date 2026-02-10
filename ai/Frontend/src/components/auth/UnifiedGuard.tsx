'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';

import { useUnifiedAuth, UserType } from '@/hooks/useUnifiedAuth';

interface UnifiedGuardProps {
  children: React.ReactNode;
  requiredUserType?: UserType;
  fallbackPath?: string;
}

/**
 * Unified guard that handles both candidate and recruiter authentication
 * No flashing, no complex redirects - just smooth authentication
 */
export function UnifiedGuard({ 
  children, 
  requiredUserType,
  fallbackPath 
}: UnifiedGuardProps) {
  const { user, userType, isLoading, isAuthenticated } = useUnifiedAuth();
  const router = useRouter();
  const [isAuthorized, setIsAuthorized] = useState<boolean | null>(null);

  useEffect(() => {
    // Wait for auth to finish loading
    if (isLoading) {
      return;
    }

    // Check if user is authenticated
    if (!isAuthenticated) {
      setIsAuthorized(false);
      return;
    }

    // If no specific user type required, allow any authenticated user
    if (!requiredUserType) {
      setIsAuthorized(true);
      return;
    }

    // Check if user type matches requirement
    if (userType === requiredUserType) {
      setIsAuthorized(true);
    } else {
      // Redirect to fallback path if provided
      setIsAuthorized(false);
    }
  }, [isAuthenticated, userType, requiredUserType, isLoading, fallbackPath, router, user]);

  // Show loading state while checking authorization
  if (isLoading || isAuthorized === null) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-muted-foreground">Loading...</p>
        </div>
      </div>
    );
  }

  // If not authorized, show access denied page
  if (isAuthorized === false) {
    const isUserTypeMismatch = isAuthenticated && requiredUserType && userType !== requiredUserType;
    
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50">
        <div className="text-center max-w-md mx-auto p-6">
          <div className="text-6xl mb-4">🔒</div>
          <h1 className="text-2xl font-bold text-red-600 mb-2">
            {!isAuthenticated ? 'Authentication Required' : 'Access Denied'}
          </h1>
          <p className="text-gray-600 mb-6">
            {!isAuthenticated 
              ? 'Please sign in to access this page.'
              : isUserTypeMismatch
                ? `This page is only accessible to ${requiredUserType}s. You are currently signed in as a ${userType}.`
                : 'You do not have permission to access this page.'
            }
          </p>
          
          <div className="space-y-3">
            <button
              onClick={() => router.back()}
              className="w-full px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
            >
              ← Go Back
            </button>
            <button
              onClick={() => router.push('/')}
              className="w-full px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300 transition-colors"
            >
              Go to Home
            </button>
            {isUserTypeMismatch && (
              <button
                onClick={() => router.push(userType === 'recruiter' ? '/recruiter/dashboard' : '/me')}
                className="w-full px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 transition-colors"
              >
                Go to My Dashboard
              </button>
            )}
          </div>
        </div>
      </div>
    );
  }

  // User is authorized, show content
  return <>{children}</>;
}
