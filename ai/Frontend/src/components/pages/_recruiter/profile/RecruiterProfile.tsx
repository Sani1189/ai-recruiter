"use client";

import { User, Mail, Building, Shield, CheckCircle } from "lucide-react";
import { useEffect } from "react";
import { toast } from "sonner";
import { useRouter } from "next/navigation";

import { useAuthStore } from "@/stores/useAuthStore";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";

export default function RecruiterProfile() {
  const { user } = useAuthStore();
  const { isLoading } = useUnifiedAuth();
  const router = useRouter();

  // Check authentication
  useEffect(() => {
    if (isLoading) return; // Wait for auth to load

    const currentUser = user;

    if (!currentUser) {
      // User not authenticated, redirect to sign in
      toast.error('Authentication required', {
        description: 'Please sign in to access your profile.',
      });
      router.push('/sign-in');
      return;
    }
  }, [user, isLoading, router]);

  // Show loading while checking auth
  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-muted-foreground">Loading your profile...</p>
        </div>
      </div>
    );
  }

  // Show loading if no authenticated user
  const currentUser = user;
  if (!currentUser) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <p className="text-muted-foreground">Redirecting to sign in...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="from-primary/5 via-background to-secondary/5 min-h-screen bg-gradient-to-br py-12">
      <div className="container px-20">
        <div className="mb-8 space-y-2">
          <h1 className="text-3xl font-bold">Profile</h1>
          <p className="text-muted-foreground">
            Your account information and access details
          </p>
        </div>

        <div className="max-w-4xl mx-auto">
          {/* Main Profile Card */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <User className="h-5 w-5" />
                Account Information
              </CardTitle>
              <CardDescription>
                Your basic account details from Azure B2C
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-8">
              {/* Profile Header */}
              <div className="flex items-center gap-6">
                <Avatar className="h-20 w-20">
                  <AvatarImage src={user?.profilePictureUrl ?? undefined} />
                  <AvatarFallback className="text-xl">
                    {currentUser.name?.charAt(0).toUpperCase() || "U"}
                  </AvatarFallback>
                </Avatar>
                <div className="flex-1">
                  <h2 className="text-2xl font-semibold">{currentUser.name ?? ""}</h2>
                  <p className="text-muted-foreground text-lg">{currentUser.email ?? ""}</p>
                  {currentUser?.roles && Array.isArray(currentUser.roles) && currentUser.roles.length > 0 && (
                    <div className="mt-3 flex gap-2">
                      {currentUser.roles.map((role: string) => (
                        <Badge key={role} variant="secondary" className="gap-1">
                          <Shield className="h-3 w-3" />
                          {role}
                        </Badge>
                      ))}
                    </div>
                  )}
                </div>
              </div>

              <Separator />

              {/* Essential Information */}
              <div className="grid gap-6 md:grid-cols-2">
                <div className="space-y-4">
                  <h3 className="font-semibold text-lg">Contact Information</h3>
                  
                  <div className="space-y-3">
                    <div className="flex items-center gap-3">
                      <Mail className="h-4 w-4 text-muted-foreground" />
                      <div>
                        <div className="text-sm text-muted-foreground">Email Address</div>
                        <div className="font-medium">{currentUser.email}</div>
                      </div>
                    </div>

                    <div className="flex items-center gap-3">
                      <Building className="h-4 w-4 text-muted-foreground" />
                      <div>
                        <div className="text-sm text-muted-foreground">Company</div>
                        <div className="font-medium">
                          {currentUser.email?.endsWith('@osilion.no') ? 'Osilion' : 'External Partner'}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <div className="space-y-4">
                  <h3 className="font-semibold text-lg">Account Status</h3>
                  
                  <div className="space-y-3">
                    <div className="flex items-center gap-3">
                      <CheckCircle className="h-4 w-4 text-green-500" />
                      <div>
                        <div className="text-sm text-muted-foreground">Status</div>
                        <div className="font-medium text-green-600">Active Recruiter</div>
                      </div>
                    </div>

                    <div className="flex items-center gap-3">
                      <Shield className="h-4 w-4 text-muted-foreground" />
                      <div>
                        <div className="text-sm text-muted-foreground">Permissions</div>
                        <div className="font-medium">Full Access</div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
