import { apiClient } from "../client"
import type { ApiResponse } from "../config"

export interface UserProfileDto {
  id: string
  name: string
  email: string
  phoneNumber?: string
  age?: number
  nationality?: string
  profilePictureUrl?: string
  bio?: string
  headline?: string
  jobTypePreferences: string[]
  openToRelocation?: boolean
  remotePreferences: string[]
  resumeUrl?: string
  createdAt: string
  updatedAt: string
}

export interface EducationDto {
  id: string
  userProfileId: string
  degree?: string
  institution?: string
  fieldOfStudy?: string
  location?: string
  startDate?: string
  endDate?: string
  createdAt: string
  updatedAt: string
}

export interface SkillDto {
  id: string
  userProfileId: string
  category?: string
  skillName: string
  proficiency?: string
  yearsExperience?: number
  unit?: string
  createdAt: string
  updatedAt: string
}

export interface ExperienceDto {
  id: string
  userProfileId: string
  title?: string
  organization?: string
  industry?: string
  location?: string
  startDate?: string
  endDate?: string
  description?: string
  createdAt: string
  updatedAt: string
}

export interface AwardAchievementDto {
  id: string
  userProfileId: string
  title?: string
  issuer?: string
  year?: number
  description?: string
  createdAt: string
  updatedAt: string
}

export interface CertificationLicenseDto {
  id: string
  userProfileId: string
  name?: string
  issuer?: string
  dateIssued?: string
  validUntil?: string
  createdAt: string
  updatedAt: string
}

export interface KeyStrengthDto {
  id: string
  userProfileId: string
  strengthName?: string
  description?: string
  createdAt: string
  updatedAt: string
}

export interface SummaryDto {
  id: string
  userProfileId: string
  type?: string
  text?: string
  createdAt: string
  updatedAt: string
}

export interface VolunteerExtracurricularDto {
  id: string
  userProfileId: string
  role?: string
  organization?: string
  startDate?: string
  endDate?: string
  description?: string
  createdAt: string
  updatedAt: string
}

export interface UserProfileDetailsDto {
  userProfile: UserProfileDto
  educations?: EducationDto[]
  skills?: SkillDto[]
  summaries?: SummaryDto[]
  awardAchievements?: AwardAchievementDto[]
  certificationLicenses?: CertificationLicenseDto[]
  experiences?: ExperienceDto[]
  projectResearch?: any[] // ProjectResearchDto if available
  keyStrengths?: KeyStrengthDto[]
  volunteerExtracurriculars?: VolunteerExtracurricularDto[]
}

class ProfileService {
  /**
   * Get current user's profile
   * Uses token from auth store
   */
  async getUserProfile(token: string): Promise<UserProfileDto> {
    console.log("ProfileService: Fetching user profile...")
    console.log("Token available:", !!token)
    try {
      const response = await apiClient.get<ApiResponse<UserProfileDto> | UserProfileDto>(
        "/userprofile/me",
        { requireAuth: true },
        token,
      )
      const profile =
        response && typeof response === "object" && "data" in response
          ? (response as ApiResponse<UserProfileDto>).data
          : (response as UserProfileDto)
      console.log("ProfileService: User profile fetched successfully:", profile)
      return profile
    } catch (error) {
      console.error("ProfileService: Failed to fetch user profile:", error)
      throw error
    }
  }

  /**
   * Get complete user profile details with all related data in a single call
   * This minimizes API calls by returning all profile sections at once
   */
  async getUserProfileDetails(token: string): Promise<UserProfileDetailsDto> {
    console.log("ProfileService: Fetching complete user profile details...")
    console.log("Token available:", !!token)
    try {
      const response = await apiClient.get<ApiResponse<UserProfileDetailsDto> | UserProfileDetailsDto>(
        "/userprofile/me/details",
        { requireAuth: true },
        token,
      )
      const details =
        response && typeof response === "object" && "data" in response
          ? (response as ApiResponse<UserProfileDetailsDto>).data
          : (response as UserProfileDetailsDto)
      console.log("ProfileService: Complete profile details fetched successfully:", details)
      return details
    } catch (error) {
      console.error("ProfileService: Failed to fetch complete profile details:", error)
      throw error
    }
  }

  /**
   * Update current user's profile
   */
  async updateUserProfile(token: string, data: Partial<UserProfileDto>) {
    console.log("ProfileService: Updating user profile with data:", data)
    try {
      const response = await apiClient.put<UserProfileDto>("/userprofile/me", data, { requireAuth: true }, token)
      console.log("ProfileService: User profile updated successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to update user profile:", error)
      throw error
    }
  }

  /**
   * Get all education records for current user
   */
  async getUserEducation(token: string) {
    console.log("ProfileService: Fetching education for current user")
    console.log("Token available:", !!token)
    try {
      const data = await apiClient.get<EducationDto[]>(
        `/education/user-profile`,
        { requireAuth: true, cacheStrategy: "cache-first", cacheTTL: 600000 },
        token,
      )
      console.log("ProfileService: Education fetched successfully:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("ProfileService: Failed to fetch education:", error)
      return []
    }
  }

  /**
   * Get education records by userProfileId (admin endpoint for viewing candidate data)
   */
  async getEducationByUserProfileId(token: string, userProfileId: string) {
    console.log("ProfileService: Fetching education for userProfileId:", userProfileId)
    try {
      const data = await apiClient.get<EducationDto[]>(
        `/education/user-profile/${userProfileId}`,
        { requireAuth: true, cacheStrategy: "cache-first", cacheTTL: 600000 },
        token,
      )
      console.log("ProfileService: Education fetched successfully:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("ProfileService: Failed to fetch education:", error)
      return []
    }
  }

  /**
   * Add new education record
   */
  async addEducation(token: string, data: Omit<EducationDto, "id" | "createdAt" | "updatedAt">) {
    console.log("ProfileService: Adding education:", data)
    try {
      const response = await apiClient.post<EducationDto>("/education", data, { requireAuth: true }, token)
      console.log("ProfileService: Education added successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to add education:", error)
      throw error
    }
  }

  /**
   * Update education record
   */
  async updateEducation(token: string, id: string, data: Partial<EducationDto>) {
    console.log("ProfileService: Updating education:", id, data)
    try {
      const response = await apiClient.put<EducationDto>(`/education/${id}`, data, { requireAuth: true }, token)
      console.log("ProfileService: Education updated successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to update education:", error)
      throw error
    }
  }

  /**
   * Delete education record
   */
  async deleteEducation(token: string, id: string) {
    console.log("ProfileService: Deleting education:", id)
    try {
      await apiClient.delete(`/education/${id}`, { requireAuth: true }, token)
      console.log("ProfileService: Education deleted successfully")
    } catch (error) {
      console.error("ProfileService: Failed to delete education:", error)
      throw error
    }
  }

  /**
   * Get all skills for current user
   */
  async getUserSkills(token: string) {
    console.log("ProfileService: Fetching skills for current user")
    console.log("Token available:", !!token)
    try {
      const data = await apiClient.get<SkillDto[]>(
        `/skill/user-profile`,
        { requireAuth: true, cacheStrategy: "cache-first", cacheTTL: 600000 },
        token,
      )
      console.log("ProfileService: Skills fetched successfully:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("ProfileService: Failed to fetch skills:", error)
      return []
    }
  }

  /**
   * Get skills by userProfileId (admin endpoint for viewing candidate data)
   */
  async getSkillsByUserProfileId(token: string, userProfileId: string) {
    console.log("ProfileService: Fetching skills for userProfileId:", userProfileId)
    try {
      const data = await apiClient.get<SkillDto[]>(
        `/skill/user-profile/${userProfileId}`,
        { requireAuth: true, cacheStrategy: "cache-first", cacheTTL: 600000 },
        token,
      )
      console.log("ProfileService: Skills fetched successfully:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("ProfileService: Failed to fetch skills:", error)
      return []
    }
  }

  /**
   * Add new skill
   */
  async addSkill(token: string, data: Omit<SkillDto, "id" | "createdAt" | "updatedAt">) {
    console.log("ProfileService: Adding skill:", data)
    try {
      const response = await apiClient.post<SkillDto>("/skill", data, { requireAuth: true }, token)
      console.log("ProfileService: Skill added successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to add skill:", error)
      throw error
    }
  }

  /**
   * Update skill
   */
  async updateSkill(token: string, id: string, data: Partial<SkillDto>) {
    console.log("ProfileService: Updating skill:", id, data)
    try {
      const response = await apiClient.put<SkillDto>(`/skill/${id}`, data, { requireAuth: true }, token)
      console.log("ProfileService: Skill updated successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to update skill:", error)
      throw error
    }
  }

  /**
   * Delete skill
   */
  async deleteSkill(token: string, id: string) {
    console.log("ProfileService: Deleting skill:", id)
    try {
      await apiClient.delete(`/skill/${id}`, { requireAuth: true }, token)
      console.log("ProfileService: Skill deleted successfully")
    } catch (error) {
      console.error("ProfileService: Failed to delete skill:", error)
      throw error
    }
  }

  /**
   * Get all experiences for current user
   */
  async getUserExperience(token: string) {
    console.log("ProfileService: Fetching experience for current user")
    console.log("Token available:", !!token)
    try {
      const data = await apiClient.get<ExperienceDto[]>(
        `/experience/user-profile`,
        { requireAuth: true, cacheStrategy: "cache-first", cacheTTL: 600000 },
        token,
      )
      console.log("ProfileService: Experience fetched successfully:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("ProfileService: Failed to fetch experience:", error)
      return []
    }
  }

  /**
   * Get experiences by userProfileId (admin endpoint for viewing candidate data)
   */
  async getExperienceByUserProfileId(token: string, userProfileId: string) {
    console.log("ProfileService: Fetching experience for userProfileId:", userProfileId)
    try {
      const data = await apiClient.get<ExperienceDto[]>(
        `/experience/user-profile/${userProfileId}`,
        { requireAuth: true, cacheStrategy: "cache-first", cacheTTL: 600000 },
        token,
      )
      console.log("ProfileService: Experience fetched successfully:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("ProfileService: Failed to fetch experience:", error)
      return []
    }
  }

  /**
   * Add new experience
   */
  async addExperience(token: string, data: Omit<ExperienceDto, "id" | "createdAt" | "updatedAt">) {
    console.log("ProfileService: Adding experience:", data)
    try {
      const response = await apiClient.post<ExperienceDto>("/experience", data, { requireAuth: true }, token)
      console.log("ProfileService: Experience added successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to add experience:", error)
      throw error
    }
  }

  /**
   * Update experience
   */
  async updateExperience(token: string, id: string, data: Partial<ExperienceDto>) {
    console.log("ProfileService: Updating experience:", id, data)
    try {
      const response = await apiClient.put<ExperienceDto>(`/experience/${id}`, data, { requireAuth: true }, token)
      console.log("ProfileService: Experience updated successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to update experience:", error)
      throw error
    }
  }

  /**
   * Delete experience
   */
  async deleteExperience(token: string, id: string) {
    console.log("ProfileService: Deleting experience:", id)
    try {
      await apiClient.delete(`/experience/${id}`, { requireAuth: true }, token)
      console.log("ProfileService: Experience deleted successfully")
    } catch (error) {
      console.error("ProfileService: Failed to delete experience:", error)
      throw error
    }
  }

  async getUserAwardAchievements(token: string) {
    console.log("ProfileService: Fetching award achievements for current user")
    try {
      const data = await apiClient.get<AwardAchievementDto[]>(
        `/awardachievement/user-profile`,
        { requireAuth: true, cacheStrategy: "cache-first", cacheTTL: 600000 },
        token,
      )
      console.log("ProfileService: Award achievements fetched successfully:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("ProfileService: Failed to fetch award achievements:", error)
      return []
    }
  }

  async addAwardAchievement(token: string, data: Omit<AwardAchievementDto, "id" | "createdAt" | "updatedAt">) {
    console.log("ProfileService: Adding award achievement:", data)
    try {
      const response = await apiClient.post<AwardAchievementDto>(
        "/awardachievement",
        data,
        { requireAuth: true },
        token,
      )
      console.log("ProfileService: Award achievement added successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to add award achievement:", error)
      throw error
    }
  }

  async updateAwardAchievement(token: string, id: string, data: Partial<AwardAchievementDto>) {
    console.log("ProfileService: Updating award achievement:", id, data)
    try {
      const response = await apiClient.put<AwardAchievementDto>(
        `/awardachievement/${id}`,
        data,
        { requireAuth: true },
        token,
      )
      console.log("ProfileService: Award achievement updated successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to update award achievement:", error)
      throw error
    }
  }

  async deleteAwardAchievement(token: string, id: string) {
    console.log("ProfileService: Deleting award achievement:", id)
    try {
      await apiClient.delete(`/awardachievement/${id}`, { requireAuth: true }, token)
      console.log("ProfileService: Award achievement deleted successfully")
    } catch (error) {
      console.error("ProfileService: Failed to delete award achievement:", error)
      throw error
    }
  }

  async getUserCertifications(token: string) {
    console.log("ProfileService: Fetching certifications for current user")
    try {
      const data = await apiClient.get<CertificationLicenseDto[]>(
        `/certificationlicense/user-profile`,
        { requireAuth: true, cacheStrategy: "cache-first", cacheTTL: 600000 },
        token,
      )
      console.log("ProfileService: Certifications fetched successfully:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("ProfileService: Failed to fetch certifications:", error)
      return []
    }
  }

  async addCertification(token: string, data: Omit<CertificationLicenseDto, "id" | "createdAt" | "updatedAt">) {
    console.log("ProfileService: Adding certification:", data)
    try {
      const response = await apiClient.post<CertificationLicenseDto>(
        "/certificationlicense",
        data,
        { requireAuth: true },
        token,
      )
      console.log("ProfileService: Certification added successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to add certification:", error)
      throw error
    }
  }

  async updateCertification(token: string, id: string, data: Partial<CertificationLicenseDto>) {
    console.log("ProfileService: Updating certification:", id, data)
    try {
      const response = await apiClient.put<CertificationLicenseDto>(
        `/certificationlicense/${id}`,
        data,
        { requireAuth: true },
        token,
      )
      console.log("ProfileService: Certification updated successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to update certification:", error)
      throw error
    }
  }

  async deleteCertification(token: string, id: string) {
    console.log("ProfileService: Deleting certification:", id)
    try {
      await apiClient.delete(`/certificationlicense/${id}`, { requireAuth: true }, token)
      console.log("ProfileService: Certification deleted successfully")
    } catch (error) {
      console.error("ProfileService: Failed to delete certification:", error)
      throw error
    }
  }

  async getUserKeyStrengths(token: string) {
    console.log("ProfileService: Fetching key strengths for current user")
    try {
      const data = await apiClient.get<KeyStrengthDto[]>(
        `/keystrength/user-profile`,
        { requireAuth: true, cacheStrategy: "cache-first", cacheTTL: 600000 },
        token,
      )
      console.log("ProfileService: Key strengths fetched successfully:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("ProfileService: Failed to fetch key strengths:", error)
      return []
    }
  }

  async addKeyStrength(token: string, data: Omit<KeyStrengthDto, "id" | "createdAt" | "updatedAt">) {
    console.log("ProfileService: Adding key strength:", data)
    try {
      const response = await apiClient.post<KeyStrengthDto>("/keystrength", data, { requireAuth: true }, token)
      console.log("ProfileService: Key strength added successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to add key strength:", error)
      throw error
    }
  }

  async updateKeyStrength(token: string, id: string, data: Partial<KeyStrengthDto>) {
    console.log("ProfileService: Updating key strength:", id, data)
    try {
      const response = await apiClient.put<KeyStrengthDto>(`/keystrength/${id}`, data, { requireAuth: true }, token)
      console.log("ProfileService: Key strength updated successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to update key strength:", error)
      throw error
    }
  }

  async deleteKeyStrength(token: string, id: string) {
    console.log("ProfileService: Deleting key strength:", id)
    try {
      await apiClient.delete(`/keystrength/${id}`, { requireAuth: true }, token)
      console.log("ProfileService: Key strength deleted successfully")
    } catch (error) {
      console.error("ProfileService: Failed to delete key strength:", error)
      throw error
    }
  }

  async getUserSummaries(token: string) {
    console.log("ProfileService: Fetching summaries for current user")
    try {
      const data = await apiClient.get<SummaryDto[]>(
        `/summary/user-profile`,
        { requireAuth: true, cacheStrategy: "cache-first", cacheTTL: 600000 },
        token,
      )
      console.log("ProfileService: Summaries fetched successfully:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("ProfileService: Failed to fetch summaries:", error)
      return []
    }
  }

  async addSummary(token: string, data: Omit<SummaryDto, "id" | "createdAt" | "updatedAt">) {
    console.log("ProfileService: Adding summary:", data)
    try {
      const response = await apiClient.post<SummaryDto>("/summary", data, { requireAuth: true }, token)
      console.log("ProfileService: Summary added successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to add summary:", error)
      throw error
    }
  }

  async updateSummary(token: string, id: string, data: Partial<SummaryDto>) {
    console.log("ProfileService: Updating summary:", id, data)
    try {
      const response = await apiClient.put<SummaryDto>(`/summary/${id}`, data, { requireAuth: true }, token)
      console.log("ProfileService: Summary updated successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to update summary:", error)
      throw error
    }
  }

  async deleteSummary(token: string, id: string) {
    console.log("ProfileService: Deleting summary:", id)
    try {
      await apiClient.delete(`/summary/${id}`, { requireAuth: true }, token)
      console.log("ProfileService: Summary deleted successfully")
    } catch (error) {
      console.error("ProfileService: Failed to delete summary:", error)
      throw error
    }
  }

  async getUserVolunteerActivities(token: string) {
    console.log("ProfileService: Fetching volunteer activities for current user")
    try {
      const data = await apiClient.get<VolunteerExtracurricularDto[]>(
        `/volunteerextracurricular/user-profile`,
        { requireAuth: true, cacheStrategy: "cache-first", cacheTTL: 600000 },
        token,
      )
      console.log("ProfileService: Volunteer activities fetched successfully:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("ProfileService: Failed to fetch volunteer activities:", error)
      return []
    }
  }

  async addVolunteerActivity(token: string, data: Omit<VolunteerExtracurricularDto, "id" | "createdAt" | "updatedAt">) {
    console.log("ProfileService: Adding volunteer activity:", data)
    try {
      const response = await apiClient.post<VolunteerExtracurricularDto>(
        "/volunteerextracurricular",
        data,
        { requireAuth: true },
        token,
      )
      console.log("ProfileService: Volunteer activity added successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to add volunteer activity:", error)
      throw error
    }
  }

  async updateVolunteerActivity(token: string, id: string, data: Partial<VolunteerExtracurricularDto>) {
    console.log("ProfileService: Updating volunteer activity:", id, data)
    try {
      const response = await apiClient.put<VolunteerExtracurricularDto>(
        `/volunteerextracurricular/${id}`,
        data,
        { requireAuth: true },
        token,
      )
      console.log("ProfileService: Volunteer activity updated successfully:", response)
      return response
    } catch (error) {
      console.error("ProfileService: Failed to update volunteer activity:", error)
      throw error
    }
  }

  async deleteVolunteerActivity(token: string, id: string) {
    console.log("ProfileService: Deleting volunteer activity:", id)
    try {
      await apiClient.delete(`/volunteerextracurricular/${id}`, { requireAuth: true }, token)
      console.log("ProfileService: Volunteer activity deleted successfully")
    } catch (error) {
      console.error("ProfileService: Failed to delete volunteer activity:", error)
      throw error
    }
  }
}

export const profileService = new ProfileService()
