"use client";

import { Suspense, useEffect } from "react";
import { Loader2, Shield, ArrowLeft } from "lucide-react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";

import BackButton from "@/components/ui/back-button";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";
import { useAuthStore } from "@/stores/useAuthStore";

function RecruiterSignInContent() {
  const { user, userType } = useAuthStore();
  const { login, isLoading } = useUnifiedAuth();
  const router = useRouter();
  const searchParams = useSearchParams();
  const redirectUrl = searchParams.get('redirect');

  useEffect(() => {
    // Only redirect if user is authenticated AND is a recruiter
    // This prevents redirect loops after logout
    if (user && userType === 'recruiter') {
      // Use redirect URL if provided, otherwise default to dashboard
      const targetUrl = redirectUrl || '/recruiter/dashboard';
      // Add a longer delay to ensure logout has completed
      const timer = setTimeout(() => {
        router.push(targetUrl);
      }, 500);
      return () => clearTimeout(timer);
    }
  }, [user, userType, router, redirectUrl]);

  // Prevent automatic re-authentication on sign-in page
  useEffect(() => {
    // If we're on the sign-in page and there's no user in the store,
    // we should stay here and not auto-redirect
  }, [user, userType]);

  const handleMicrosoftLogin = () => {
    // Use 'redirect' instead of 'popup' to use same browser window
    login('recruiter', 'redirect', redirectUrl || '/recruiter/dashboard');
  };

  return (
    <div className="from-primary/5 via-background to-secondary/5 flex items-center justify-center bg-gradient-to-br py-12">
      <div className="w-full max-w-md">
        <BackButton />

        <Card>
          <CardHeader className="text-center">
            <CardTitle className="text-2xl">Recruiter Admin</CardTitle>
            <CardDescription>
              Sign in to access your recruitment dashboard
            </CardDescription>
          </CardHeader>

          <CardContent className="space-y-6">
            {/* Microsoft Sign In */}
            <Button
              variant="outline"
              className="w-full"
              onClick={handleMicrosoftLogin}
              disabled={isLoading}
            >
              {isLoading ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Shield className="mr-2 h-4 w-4" />
              )}
              Continue with Microsoft
            </Button>

            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <span className="w-full border-t" />
              </div>
              <div className="relative flex justify-center text-xs uppercase">
                <Badge variant="secondary" className="text-muted-foreground">
                  Secure Authentication
                </Badge>
              </div>
            </div>

            <div className="space-y-3 text-center text-sm">
              <div className="text-muted-foreground">
                <p className="text-xs">
                  For recruiters and hiring managers with admin access only
                </p>
                <p className="text-xs mt-1 text-amber-600">
                  ⚠️ You must have RecruitmentAdmin role to access this area
                </p>
              </div>

              <div>
                <span className="text-muted-foreground">
                  Are you a candidate?{" "}
                </span>
                <Link href="/sign-in" className="text-primary hover:underline">
                  Sign in here
                </Link>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export default function RecruiterSignInPage() {
  return (
    <Suspense
      fallback={
        <div className="flex min-h-screen items-center justify-center">
          <Loader2 className="h-6 w-6 animate-spin" />
        </div>
      }
    >
      <RecruiterSignInContent />
    </Suspense>
  );
}