"use client"
import { useEffect, useState } from "react"
import { toast } from "sonner"
import { useRouter } from "next/navigation"
import { FileText, Edit2, Download, Award, Briefcase, BookOpen, Zap, Star, Heart, Globe } from "lucide-react"

import ProfileDisplaySection from "@/components/pages/_candidate/profile/ProfileDisplaySection"
import EducationDisplaySection from "@/components/pages/_candidate/profile/EducationDisplaySection"
import SkillsDisplaySection from "@/components/pages/_candidate/profile/SkillsDisplaySection"
import ExperienceDisplaySection from "@/components/pages/_candidate/profile/ExperienceDisplaySection"
import AwardAchievementDisplaySection from "@/components/pages/_candidate/profile/AwardAchievementDisplaySection"
import CertificationDisplaySection from "@/components/pages/_candidate/profile/CertificationDisplaySection"
import KeyStrengthDisplaySection from "@/components/pages/_candidate/profile/KeyStrengthDisplaySection"
import SummaryDisplaySection from "@/components/pages/_candidate/profile/SummaryDisplaySection"
import VolunteerDisplaySection from "@/components/pages/_candidate/profile/VolunteerDisplaySection"

import { useAuthStore } from "@/stores/useAuthStore"
import { useProfileData } from "@/hooks/useProfileData"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"

const SECTIONS = [
  { id: "basic", label: "Overview", icon: Globe },
  { id: "summary", label: "About", icon: FileText },
  { id: "experience", label: "Experience", icon: Briefcase },
  { id: "education", label: "Education", icon: BookOpen },
  { id: "skills", label: "Skills", icon: Zap },
  { id: "awards", label: "Awards", icon: Award },
  { id: "certifications", label: "Certifications", icon: Star },
  { id: "strengths", label: "Strengths", icon: Heart },
  { id: "volunteer", label: "Volunteer", icon: Heart },
]

export default function CandidateProfilePage() {
  const { user } = useAuthStore()
  const router = useRouter()
  const {
    profile: profileData,
    education,
    skills,
    experience,
    awards,
    certifications,
    strengths,
    summaries,
    volunteers,
    cvFile,
    isLoading,
    error,
    refreshAll,
  } = useProfileData()

  const [activeTab, setActiveTab] = useState("basic")

  useEffect(() => {
    if (!isLoading && !user) {
      toast.error("Authentication required")
      router.push("/sign-in")
    }
  }, [isLoading, user, router, error])

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-4 border-primary border-t-transparent mx-auto mb-4"></div>
          <p className="text-muted-foreground">Loading your profile...</p>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <p className="text-destructive">Failed to load profile</p>
          <button onClick={refreshAll} className="mt-4 text-primary hover:underline">
            Try Again
          </button>
        </div>
      </div>
    )
  }

  if (!user) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <p className="text-muted-foreground">Redirecting to sign in...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-background">
      {/* Hero Section */}
      <div className="relative overflow-hidden border-b border-border/40 bg-gradient-to-br from-primary/10 via-transparent to-secondary/10">
        <div className="absolute inset-0 -z-10 opacity-30">
          <div className="absolute h-96 w-96 rounded-full bg-primary/20 blur-3xl -top-40 -left-40"></div>
          <div className="absolute h-96 w-96 rounded-full bg-secondary/20 blur-3xl -bottom-40 -right-40"></div>
        </div>

        <div className="max-w-7xl mx-auto px-6 lg:px-8 py-12 md:py-16">
          <div className="flex flex-col md:flex-row items-start md:items-center gap-8 md:gap-12">
            {/* Avatar */}
            <div className="relative">
              <div className="absolute inset-0 bg-gradient-to-br from-primary/40 to-secondary/40 rounded-2xl blur-xl"></div>
              <div className="relative w-32 h-32 md:w-40 md:h-40 rounded-2xl bg-gradient-to-br from-primary to-secondary flex items-center justify-center flex-shrink-0 shadow-lg border border-primary/20">
                <span className="text-6xl md:text-7xl font-bold text-white">
                  {user?.name?.slice(0, 1).toUpperCase()}
                </span>
              </div>
            </div>

            {/* User Info */}
            <div className="flex-1">
              <h1 className="text-4xl md:text-5xl font-bold text-foreground mb-3 tracking-tight">
                {user?.name || "Your Name"}
              </h1>
              <p className="text-lg text-primary font-semibold mb-2">
                {profileData?.headline || "Professional Profile"}
              </p>
              <p className="text-muted-foreground mb-6">{user?.email}</p>

              {/* Action Buttons */}
              <div className="flex flex-wrap gap-3">
                <Button className="gap-2">
                  <Edit2 className="h-4 w-4" />
                  Edit Profile
                </Button>
                {cvFile && (
                  <Button variant="outline" className="gap-2 bg-transparent">
                    <Download className="h-4 w-4" />
                    Download CV
                  </Button>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Stats Section */}
      <div className="max-w-7xl mx-auto px-6 lg:px-8 py-8">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <Card className="p-4 text-center border-border/40">
            <div className="text-2xl font-bold text-primary">{experience?.length || 0}</div>
            <p className="text-sm text-muted-foreground">Experiences</p>
          </Card>
          <Card className="p-4 text-center border-border/40">
            <div className="text-2xl font-bold text-primary">{education?.length || 0}</div>
            <p className="text-sm text-muted-foreground">Education</p>
          </Card>
          <Card className="p-4 text-center border-border/40">
            <div className="text-2xl font-bold text-primary">{skills?.length || 0}</div>
            <p className="text-sm text-muted-foreground">Skills</p>
          </Card>
          <Card className="p-4 text-center border-border/40">
            <div className="text-2xl font-bold text-primary">{certifications?.length || 0}</div>
            <p className="text-sm text-muted-foreground">Certifications</p>
          </Card>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div className="sticky top-0 z-30 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 border-b border-border/40">
        <div className="max-w-7xl mx-auto px-6 lg:px-8">
          <div className="flex overflow-x-auto gap-1 md:gap-2">
            {SECTIONS.map((section) => {
              const Icon = section.icon
              return (
                <button
                  key={section.id}
                  onClick={() => setActiveTab(section.id)}
                  className={`flex items-center gap-2 px-4 py-3 text-sm md:text-base font-medium whitespace-nowrap transition-all duration-300 border-b-2 ${
                    activeTab === section.id
                      ? "border-primary text-primary"
                      : "border-transparent text-muted-foreground hover:text-foreground"
                  }`}
                >
                  <Icon className="h-4 w-4" />
                  <span className="hidden sm:inline">{section.label}</span>
                </button>
              )
            })}
          </div>
        </div>
      </div>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-6 lg:px-8 py-12 md:py-16">
        <div className="space-y-8">
          {activeTab === "basic" && profileData && (
            <ProfileDisplaySection profile={profileData} cvFile={cvFile} onRefresh={refreshAll} />
          )}
          {activeTab === "summary" && (
            <SummaryDisplaySection summaries={summaries} userProfileId={profileData?.id || ""} onRefresh={refreshAll} />
          )}
          {activeTab === "experience" && (
            <ExperienceDisplaySection
              experience={experience}
              userProfileId={profileData?.id || ""}
              onRefresh={refreshAll}
            />
          )}
          {activeTab === "education" && (
            <EducationDisplaySection
              education={education}
              userProfileId={profileData?.id || ""}
              onRefresh={refreshAll}
            />
          )}
          {activeTab === "skills" && (
            <SkillsDisplaySection skills={skills} userProfileId={profileData?.id || ""} onRefresh={refreshAll} />
          )}
          {activeTab === "awards" && (
            <AwardAchievementDisplaySection
              awards={awards}
              userProfileId={profileData?.id || ""}
              onRefresh={refreshAll}
            />
          )}
          {activeTab === "certifications" && (
            <CertificationDisplaySection
              certifications={certifications}
              userProfileId={profileData?.id || ""}
              onRefresh={refreshAll}
            />
          )}
          {activeTab === "strengths" && (
            <KeyStrengthDisplaySection
              strengths={strengths}
              userProfileId={profileData?.id || ""}
              onRefresh={refreshAll}
            />
          )}
          {activeTab === "volunteer" && (
            <VolunteerDisplaySection
              volunteers={volunteers}
              userProfileId={profileData?.id || ""}
              onRefresh={refreshAll}
            />
          )}
        </div>
      </main>
    </div>
  )
}
