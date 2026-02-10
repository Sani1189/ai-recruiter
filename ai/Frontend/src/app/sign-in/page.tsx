"use client";

import React, { Suspense, useEffect, useState } from "react";
import { Loader2, Mail, Shield } from "lucide-react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { toast } from "sonner";

import Google from "@/components/icons/Google";
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

function SignInContent() {
  const { user, userType } = useAuthStore();
  const { login, isLoading } = useUnifiedAuth();
  const router = useRouter();
  const searchParams = useSearchParams();
  const redirectUrl = searchParams.get('redirect');
  const [isRedirecting, setIsRedirecting] = useState(false);

  useEffect(() => {
    // Handle redirect after login completes
    if (user && userType === 'candidate' && !isLoading && !isRedirecting) {
      // Get redirect URL from sessionStorage (stored before login) or query param, or default
      const storedRedirectUrl = typeof window !== 'undefined' 
        ? sessionStorage.getItem('authRedirectUrl') 
        : null;
      
      // Clear stored redirect URL after reading
      if (storedRedirectUrl && typeof window !== 'undefined') {
        sessionStorage.removeItem('authRedirectUrl');
      }
      
      // Use stored redirect, query param, or default
      const targetUrl = storedRedirectUrl || redirectUrl || '/profile';
      setIsRedirecting(true);
      router.push(targetUrl);
    }
  }, [user, userType, isLoading, router, redirectUrl, isRedirecting]);

  const handleAzureSignIn = async () => {
    try {
      setIsRedirecting(true);
      // Store redirect URL in sessionStorage before login
      // Use redirectUrl from query param if exists, otherwise default to /profile
      const finalRedirectUrl = redirectUrl || '/profile';
      if (typeof window !== 'undefined') {
        sessionStorage.setItem('authRedirectUrl', finalRedirectUrl);
      }
      
      await login('candidate', 'redirect');
      // Note: With redirect mode, the page will navigate away and come back
      // The redirect will be handled by this page's useEffect after login
    } catch (error) {
      console.error('Login failed:', error);
      setIsRedirecting(false);
    }
  };

  const handleSignUp = () => {
    router.push('/sign-up');
  };

  return (
    <div className="from-primary/5 via-background to-secondary/5 flex items-center justify-center bg-gradient-to-br py-12">
      <div className="w-full max-w-md">
        <BackButton />

        <Card>
          <CardHeader className="text-center">
            <CardTitle className="text-2xl">Welcome Back</CardTitle>

            <CardDescription>
              Sign in to your account to continue your journey
            </CardDescription>
          </CardHeader>

          <CardContent className="space-y-6">
            {/* Microsoft Sign In */}
            <Button
              variant="outline"
              className="w-full"
              onClick={handleAzureSignIn}
              disabled={isLoading}
            >
              {isLoading ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Shield className="mr-2 h-4 w-4" />
              )}
              Continue with Microsoft
            </Button>

            {/* Google Sign In */}
            <Button
              variant="outline"
              className="w-full"
              onClick={handleAzureSignIn}
              disabled={isLoading}
            >
              {isLoading ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Google />
              )}
              Continue with Google
            </Button>

            {/* Email Sign In */}
            <Button
              variant="outline"
              className="w-full"
              onClick={handleAzureSignIn}
              disabled={isLoading}
            >
              {isLoading ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Mail className="mr-2 h-4 w-4" />
              )}
              Continue with Email
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
                  All authentication is handled securely through Azure B2C
                </p>
              </div>

              <div>
                <span className="text-muted-foreground">
                  Don't have an account?{" "}
                </span>

                <Link href="/sign-up" className="text-primary hover:underline">
                  Sign up
                </Link>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export default function SignInPage() {
  return (
    <Suspense
      fallback={
        <div className="flex min-h-screen items-center justify-center">
          <Loader2 className="h-6 w-6 animate-spin" />
        </div>
      }
    >
      <SignInContent />
    </Suspense>
  );
}
