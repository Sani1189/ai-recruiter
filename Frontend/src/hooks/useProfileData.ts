"use client"

import { useCallback, useEffect, useRef, useState } from "react"
import { useUnifiedAuth } from "./useUnifiedAuth"
import { profileService } from "@/lib/api/services/profile.service"
import type {
  UserProfileDto,
  EducationDto,
  SkillDto,
  ExperienceDto,
  AwardAchievementDto,
  CertificationLicenseDto,
  KeyStrengthDto,
  SummaryDto,
  VolunteerExtracurricularDto,
  UserProfileDetailsDto,
} from "@/lib/api/services/profile.service"
import { toast } from "sonner"
import type { CvFileData } from "@/lib/api/services/cv.service"

interface ProfileDataState {
  profile: UserProfileDto | null
  education: EducationDto[]
  skills: SkillDto[]
  experience: ExperienceDto[]
  awards: AwardAchievementDto[]
  certifications: CertificationLicenseDto[]
  strengths: KeyStrengthDto[]
  summaries: SummaryDto[]
  volunteers: VolunteerExtracurricularDto[]
  cvFile: CvFileData | null
  isLoading: boolean
  error: Error | null
}

const initialState: ProfileDataState = {
  profile: null,
  education: [],
  skills: [],
  experience: [],
  awards: [],
  certifications: [],
  strengths: [],
  summaries: [],
  volunteers: [],
  cvFile: null,
  isLoading: true,
  error: null,
}

export function useProfileData() {
  const { user, isLoading: authLoading, getAccessToken } = useUnifiedAuth()
  const [state, setState] = useState<ProfileDataState>(initialState)
  const hasFetchedRef = useRef(false)
  const isFetchingRef = useRef(false)
  const fetchAbortControllerRef = useRef<AbortController | null>(null)
  const previousUserIdRef = useRef<string | null>(null)

  const fetchProfileData = useCallback(async () => {
    // Prevent multiple simultaneous fetches
    if (authLoading || hasFetchedRef.current || isFetchingRef.current) {
      return
    }
    
    if (!user) {
      console.log("] No user available, skipping fetch")
      return
    }

    // Mark as fetching immediately to prevent concurrent calls
    isFetchingRef.current = true
    
    try {
      setState((prev) => ({ ...prev, isLoading: true, error: null }))
      console.log("] Starting profile data fetch...")

      const token = await getAccessToken()
      if (!token) {
        throw new Error("No access token")
      }

      // Create abort controller for this fetch
      fetchAbortControllerRef.current = new AbortController()

      // Use the new single endpoint to get all profile data at once
      const profileDetails = await profileService.getUserProfileDetails(token)

      if (!profileDetails || !profileDetails.userProfile || !profileDetails.userProfile.id) {
        throw new Error("Failed to fetch user profile details or profile ID missing")
      }

      console.log("] Complete profile details fetched successfully with ID:", profileDetails.userProfile.id)

      setState({
        profile: profileDetails.userProfile,
        education: profileDetails.educations || [],
        skills: profileDetails.skills || [],
        experience: profileDetails.experiences || [],
        awards: profileDetails.awardAchievements || [],
        certifications: profileDetails.certificationLicenses || [],
        strengths: profileDetails.keyStrengths || [],
        summaries: profileDetails.summaries || [],
        volunteers: profileDetails.volunteerExtracurriculars || [],
        cvFile: null, // Not needed - resume download uses /userprofile/candidate/{id}/resume endpoint
        isLoading: false,
        error: null,
      })

      hasFetchedRef.current = true
    } catch (error) {
      if ((error as Error).name !== "AbortError") {
        const err = error instanceof Error ? error : new Error("Unknown error")
        console.error("] Profile fetch error:", err.message)
        setState((prev) => ({ ...prev, isLoading: false, error: err }))
        console.error("Failed to fetch profile data:", error)
        // Reset hasFetched on error so we can retry
        hasFetchedRef.current = false
      }
    } finally {
      isFetchingRef.current = false
    }
  }, [user, authLoading, getAccessToken])

  // Partial refresh for specific section (e.g., after updates)
  const refreshSection = useCallback(
    async (section: keyof Omit<ProfileDataState, "isLoading" | "error">) => {
      if (!user || !state.profile) return

      try {
        const token = await getAccessToken()
        if (!token) return

        let newData: any = null

        switch (section) {
          case "profile":
            newData = await profileService.getUserProfile(token)
            setState((prev) => ({ ...prev, profile: newData }))
            break
          case "education":
            newData = await profileService.getUserEducation(token)
            setState((prev) => ({ ...prev, education: newData || [] }))
            break
          case "skills":
            newData = await profileService.getUserSkills(token)
            setState((prev) => ({ ...prev, skills: newData || [] }))
            break
          case "experience":
            newData = await profileService.getUserExperience(token)
            setState((prev) => ({ ...prev, experience: newData || [] }))
            break
          case "awards":
            newData = await profileService.getUserAwardAchievements(token)
            setState((prev) => ({ ...prev, awards: newData || [] }))
            break
          case "certifications":
            newData = await profileService.getUserCertifications(token)
            setState((prev) => ({ ...prev, certifications: newData || [] }))
            break
          case "strengths":
            newData = await profileService.getUserKeyStrengths(token)
            setState((prev) => ({ ...prev, strengths: newData || [] }))
            break
          case "summaries":
            newData = await profileService.getUserSummaries(token)
            setState((prev) => ({ ...prev, summaries: newData || [] }))
            break
          case "volunteers":
            newData = await profileService.getUserVolunteerActivities(token)
            setState((prev) => ({ ...prev, volunteers: newData || [] }))
            break
          // cvFile is not refreshed - resume download uses /userprofile/candidate/{id}/resume endpoint
        }
      } catch (error) {
        console.error(`Failed to refresh ${section}:`, error)
      }
    },
    [user, getAccessToken],
  )

  // Full refresh of all data
  const refreshAll = useCallback(async () => {
    setState((prev) => ({ ...prev, isLoading: true }))
    hasFetchedRef.current = false
    isFetchingRef.current = false
    await fetchProfileData()
  }, [fetchProfileData])

  useEffect(() => {
    const currentUserId = user?.id ?? null
    
    // Reset fetch state if user changed (e.g., logout/login or user switch)
    if (previousUserIdRef.current !== null && previousUserIdRef.current !== currentUserId) {
      console.log("] User changed, resetting fetch state")
      hasFetchedRef.current = false
      isFetchingRef.current = false
      setState(initialState)
      
      // Abort any ongoing fetch
      if (fetchAbortControllerRef.current) {
        fetchAbortControllerRef.current.abort()
        fetchAbortControllerRef.current = null
      }
    }
    
    previousUserIdRef.current = currentUserId
    
    console.log(
      "] useEffect running - authLoading:",
      authLoading,
      "user:",
      !!user,
      "hasFetched:",
      hasFetchedRef.current,
      "isFetching:",
      isFetchingRef.current,
    )
    
    // Only fetch if auth is ready, user exists, hasn't been fetched, and not currently fetching
    if (!authLoading && user && !hasFetchedRef.current && !isFetchingRef.current) {
      console.log("] Triggering profile data fetch")
      fetchProfileData()
    }

    return () => {
      if (fetchAbortControllerRef.current) {
        fetchAbortControllerRef.current.abort()
        fetchAbortControllerRef.current = null
      }
    }
  }, [authLoading, user, fetchProfileData])

  return {
    ...state,
    refreshSection,
    refreshAll,
  }
}
