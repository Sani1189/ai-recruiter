"use client";

import { Loader2, Mail, Shield } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

import Google from "@/components/icons/Google";
import BackButton from "@/components/ui/back-button";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";
import { useAuthStore } from "@/stores/useAuthStore";

export default function SignUpPage() {
  const { user, userType } = useAuthStore();
  const { login, isLoading, isAuthenticated } = useUnifiedAuth();
  const router = useRouter();

  useEffect(() => {
    // Redirect if already authenticated as candidate
    if (user && userType === 'candidate') {
      router.push('/profile');
    }
  }, [user, userType, router]);

  const handleAzureSignUp = () => {
    login('candidate', 'redirect');
  };

  const handleSignIn = () => {
    router.push('/sign-in');
  };

  return (
    <div className="from-primary/5 via-background to-secondary/5 flex items-center justify-center bg-gradient-to-br py-12">
      <div className="w-full max-w-md">
        <BackButton />

        <Card>
          <CardHeader className="text-center">
            <CardTitle className="text-2xl">Create Your Account</CardTitle>

            <CardDescription>
              Join thousands of candidates and start your career journey
            </CardDescription>
          </CardHeader>

          <CardContent className="space-y-6">
            {/* Microsoft Sign Up */}
            <Button
              variant="outline"
              className="w-full"
              onClick={handleAzureSignUp}
              disabled={isLoading}
            >
              {isLoading ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Shield className="mr-2 h-4 w-4" />
              )}
              Sign up with Microsoft
            </Button>

            {/* Google Sign Up */}
            <Button
              variant="outline"
              className="w-full"
              onClick={handleAzureSignUp}
              disabled={isLoading}
            >
              {isLoading ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Google />
              )}
              Sign up with Google
            </Button>

            {/* Email Sign Up */}
            <Button
              variant="outline"
              className="w-full"
              onClick={handleAzureSignUp}
              disabled={isLoading}
            >
              {isLoading ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Mail className="mr-2 h-4 w-4" />
              )}
              Sign up with Email
            </Button>

            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <span className="w-full border-t" />
              </div>

              <div className="relative flex justify-center text-xs uppercase">
                <Badge variant="secondary" className="text-muted-foreground">
                  Secure Registration
                </Badge>
              </div>
            </div>

            <div className="space-y-3 text-center text-sm">
              <div className="text-muted-foreground">
                <p className="text-xs">
                  Registration is handled securely through Azure B2C
                </p>
                <p className="text-xs mt-1">
                  You can sign up using Microsoft, Google, or email
                </p>
              </div>

              <div>
                <span className="text-muted-foreground">
                  Already have an account?{" "}
                </span>

                <Link href="/sign-in" className="text-primary hover:underline">
                  Sign in
                </Link>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
