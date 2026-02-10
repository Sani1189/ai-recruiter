"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import {
  Briefcase,
  FolderOpen,
  GraduationCap,
  Languages,
  Loader2,
  Sparkles,
  User,
} from "lucide-react";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { useRouter } from "next/navigation";

import BasicFormSection from "@/components/pages/_candidate/me/BasicFormSection";
import EducationFormSection from "@/components/pages/_candidate/me/EducationFormSection";
import ExperienceFormSection from "@/components/pages/_candidate/me/ExperienceFormSection";
import LanguagesFormSection from "@/components/pages/_candidate/me/LanguagesFormSection";
import ProjectsFormSection from "@/components/pages/_candidate/me/ProjectsFormSection";
import SkillsFormSection from "@/components/pages/_candidate/me/SkillsFormSection";
import { Button } from "@/components/ui/button";
import { Form } from "@/components/ui/form";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

import { mockUserProfiles } from "@/dummy";
import { UserProfileSchema } from "@/schemas/user-profile";
import { useAuthStore } from "@/stores/useAuthStore";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";
import { UnifiedGuard } from "@/components/auth/UnifiedGuard";

const TABS_CONTENT = {
  triggers: [
    { value: "basic", label: "Basic Info", icon: User },
    { value: "experience", label: "Experience", icon: Briefcase },
    { value: "education", label: "Education", icon: GraduationCap },
    { value: "projects", label: "Projects", icon: FolderOpen },
    { value: "skills", label: "Skills", icon: Sparkles },
    { value: "languages", label: "Languages", icon: Languages },
  ],
  contents: [
    { value: "basic", component: BasicFormSection },
    { value: "experience", component: ExperienceFormSection },
    { value: "education", component: EducationFormSection },
    { value: "projects", component: ProjectsFormSection },
    { value: "skills", component: SkillsFormSection },
    { value: "languages", component: LanguagesFormSection },
  ],
};

export default function LoggedInUserProfilePage() {
  const { user } = useAuthStore();
  const { isLoading } = useUnifiedAuth();
  const router = useRouter();

  // Check authentication
  useEffect(() => {
    if (isLoading) return; // Wait for auth to load
    
    if (!user) {
      // User not authenticated, redirect to sign in
      toast.error('Authentication required', {
        description: 'Please sign in to access your profile.',
      });
      router.push('/sign-in');
      return;
    }
  }, [user, isLoading, router]);

  const profile = mockUserProfiles[0]; // Keep original static data
  const currentUser = user; // For authentication display only

  const form = useForm<UserProfileSchema>({
    resolver: zodResolver(UserProfileSchema),
    defaultValues: profile,
  });

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
  if (!currentUser) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <p className="text-muted-foreground">Redirecting to sign in...</p>
        </div>
      </div>
    );
  }

  const onSubmit = async (data: UserProfileSchema) => {
    try {
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 1000));

      toast.success("Profile updated", {
        description: "Your profile has been successfully updated.",
      });
    } catch (error) {
      toast.error("Update failed", {
        description: "Failed to update your profile",
      });
    }
  };

  return (
    <div className="from-primary/5 via-background to-secondary/5 min-h-screen bg-gradient-to-br py-12">
        <div className="container px-20">
          <div className="mb-8 space-y-2">
            <h1 className="text-3xl font-bold">Profile Settings</h1>

            <p className="text-muted-foreground">
              Manage your profile information and preferences
            </p>
            
            {currentUser && (
              <div className="mt-4 p-4 bg-muted/50 rounded-lg">
                <p className="text-sm text-muted-foreground">
                  <strong>Authenticated as:</strong> {currentUser.name} ({currentUser.email})
                </p>
                {currentUser?.roles && Array.isArray(currentUser.roles) && currentUser.roles.length > 0 && (
                  <p className="text-sm text-muted-foreground mt-1">
                    <strong>Roles:</strong> {currentUser.roles.join(', ')}
                  </p>
                )}
              </div>
            )}
          </div>

          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)}>
              <Tabs defaultValue="basic" className="space-y-6">
                <TabsList className="grid w-full grid-cols-6">
                  {TABS_CONTENT.triggers.map((trigger, index) => (
                    <TabsTrigger
                      key={index}
                      value={trigger.value}
                      className="flex items-center gap-2"
                    >
                      <trigger.icon className="h-4 w-4" />
                      {trigger.label}
                    </TabsTrigger>
                  ))}
                </TabsList>

                {TABS_CONTENT.contents.map((content, index) => (
                  <TabsContent key={index} value={content.value}>
                    <content.component form={form} profile={profile} />
                  </TabsContent>
                ))}
              </Tabs>

              <div className="mt-6">
                <Button
                  type="submit"
                  disabled={form.formState.isSubmitting}
                  className="w-full"
                >
                  {form.formState.isSubmitting ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Saving...
                    </>
                  ) : (
                    "Save Changes"
                  )}
                </Button>
              </div>
            </form>
          </Form>
        </div>
      </div>
  );
}
