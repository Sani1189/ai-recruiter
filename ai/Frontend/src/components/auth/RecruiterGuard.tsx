'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';

import { useAuthStore } from '@/stores/useAuthStore';
import { logAuthEvent } from '@/lib/auth/utils';

/**
 * Special guard component just for recruiter routes
 * This is a simplified version that only checks if a user exists
 * and redirects to sign-in if not
 */
export function RecruiterGuard({ 
  children 
}: {
  children: React.ReactNode;
}) {
  const { user } = useAuthStore();
  const router = useRouter();
  const [isAuthorized, setIsAuthorized] = useState<boolean | null>(null);

  useEffect(() => {
    // Simple check: if no user, redirect to sign-in
    if (!user) {
      logAuthEvent('Unauthenticated access attempt to recruiter route');
      setIsAuthorized(false);
      
      toast.error('Authentication Required', {
        description: 'Please sign in to access recruiter admin pages.'
      });
      
      router.push('/recruiter-sign-in');
    } else {
      // User exists, allow access
      setIsAuthorized(true);
    }
  }, [user, router]);

  // Show loading while checking
  if (isAuthorized === null) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-muted-foreground">Checking access...</p>
        </div>
      </div>
    );
  }

  // If not authorized, show restricted message
  if (isAuthorized === false) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="text-6xl">🔒</div>
          <h1 className="text-2xl font-bold text-red-600 mt-4">Authentication Required</h1>
          <p className="text-muted-foreground max-w-md mt-2">
            Please sign in to access recruiter admin pages.
          </p>
          <p className="text-sm text-muted-foreground mt-6">Redirecting to sign-in...</p>
        </div>
      </div>
    );
  }

  // User is authorized, show content
  return <>{children}</>;
}
