"use client"

import { Mic } from "lucide-react"
import Link from "next/link"

import LoggedInUserProfile from "./LoggedInUserProfile"
import RecruiterLoggedInUserProfile from "./RecruiterLoggedInUserProfile"
import RecruiterNavItems from "./RecruiterNavItems"
import CandidateNavItems from "./CandidateNavItems"
import { useAuthStore } from "@/stores/useAuthStore"

export default function Header({
  variant,
}: {
  variant: "recruiter" | "candidate"
}) {
  const { user, userType } = useAuthStore()
  const isAuthenticated = !!user
  const isRecruiter = userType === "recruiter"
  const isCandidate = userType === "candidate"

  // Hide header for authenticated recruiters and candidates (they use sidebar)
  if (
    (variant === "recruiter" && isAuthenticated && isRecruiter) ||
    (variant === "candidate" && isAuthenticated && isCandidate)
  ) {
    return null
  }

  return (
    <header className="bg-background/95 supports-[backdrop-filter]:bg-background/60 sticky top-0 z-50 w-full border-b backdrop-blur">
      <div className="container flex h-16 items-center justify-between">
        <Link href="/" className="flex items-center gap-2 text-xl font-bold">
          <div className="from-brand to-brand-secondary grid h-8 w-8 place-content-center rounded-lg bg-gradient-to-r">
            <Mic className="h-5 w-5 text-white" />
          </div>

          <span className="from-brand to-brand-secondary bg-gradient-to-r bg-clip-text text-transparent">
            InterviewAI
          </span>
        </Link>

        <div className="flex items-center gap-4">
          {/* Navigation items - each component handles its own authentication */}
          {/* Use key to force re-render when auth state changes */}
          {variant === "recruiter" && (
            <RecruiterNavItems key={`recruiter-nav-${isAuthenticated ? "authenticated" : "unauthenticated"}`} />
          )}
          {variant === "candidate" && (
            <CandidateNavItems key={`candidate-nav-${isAuthenticated ? "authenticated" : "unauthenticated"}`} />
          )}

          {/* User profile - shows appropriate profile based on user type */}
          {isAuthenticated && isRecruiter ? <RecruiterLoggedInUserProfile /> : <LoggedInUserProfile />}
        </div>
      </div>
    </header>
  )
}
