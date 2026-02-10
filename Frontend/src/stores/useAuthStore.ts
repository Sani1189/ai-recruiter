import { toast } from "sonner"
import { create } from "zustand"
import { persist } from "zustand/middleware"

import type { UserProfile } from "@/types/type"

interface AzureUserInfo {
  id: string
  email: string | null
  name: string | null
  roles?: string[]
}

type UserType = "candidate" | "recruiter" | null

type AuthStore = {
  user: UserProfile | null
  azureUser: AzureUserInfo | null
  userType: UserType
  isLoading: boolean
  login: (user: Pick<UserProfile, "email"> & { password: string }) => Promise<boolean>
  loginWithAzure: (azureUser: AzureUserInfo, userType: UserType) => Promise<boolean>
  register: (data: Pick<UserProfile, "email" | "name"> & { password: string }) => Promise<boolean>
  logout: () => Promise<boolean>
  initializeAuth: () => Promise<void>
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      user: null,
      azureUser: null,
      userType: null,
      isLoading: true,

      login: async ({ email, password }) => {
        try {
          // This is for demo/fallback purposes - in production, this would be removed
          // For now, we'll just return false to encourage Azure B2C usage
          toast.error("Please use Azure B2C authentication", {
            description: "Traditional login is no longer supported.",
          })
          return false
        } catch (error) {
          const message = error instanceof Error ? error.message : "Login failed. Please try again."
          console.error("Login failed:", error)
          toast.error("Failure", {
            description: message,
          })
          return false
        }
      },

      loginWithAzure: async (azureUser, userType) => {
        try {
          // Don't override recruiter authentication with candidate authentication
          const currentState = get()
          if (currentState.userType === "recruiter" && userType === "candidate") {
            return true
          }

          // Convert Azure user to our UserProfile format
          const userProfile: UserProfile = {
            id: azureUser.id,
            email: azureUser.email,
            name: azureUser.name,
            resumeUrl: "",
            // Add other required fields with defaults
            phoneNumber: "",
            age: 25,
            nationality: "",
            profilePictureUrl: "",
            bio: "",
            jobTypePreferences: ["full-time"],
            openToRelocation: false,
            remotePreferences: ["remote"],
            education: [],
            workExperience: [],
            projects: [],
            speakingLanguages: [],
            programmingLanguages: [],
            createdAt: new Date().toISOString(),
          }

          set({ user: userProfile, azureUser, userType })
          return true
        } catch (error) {
          const message = error instanceof Error ? error.message : "Azure login failed. Please try again."
          console.error("Azure login failed:", error)
          toast.error("Failure", {
            description: message,
          })
          return false
        }
      },

      register: async ({ email, password, name }) => {
        try {
          // Registration now handled by Azure B2C
          toast.info("Please use Azure B2C sign up", {
            description: "Registration is handled through Azure B2C.",
          })
          return false
        } catch (error) {
          const message = error instanceof Error ? error.message : "Registration failed. Please try again."
          console.error("Registration failed:", error)
          toast.error("Failure", {
            description: message,
          })
          return false
        }
      },

      logout: async () => {
        try {
          set({
            user: null,
            azureUser: null,
            userType: null,
            isLoading: false,
          })

          if (typeof window !== "undefined") {
            localStorage.clear()
            sessionStorage.clear()
          }

          return true
        } catch (error) {
          console.error("Logout failed:", error)
          toast.error("Failure", {
            description: "Logout failed. Please try again.",
          })
          return false
        }
      },

      initializeAuth: async () => {
        try {
          // Azure B2C handles initialization through MSAL
          // This method is kept for compatibility but Azure auth handles initialization
          set({ isLoading: false })
        } catch (err) {
          console.error("Failed to initialize auth:", err)
          toast.error("Failure", {
            description: "Failed to initialize authentication. Please try again.",
          })
          set({ user: null, azureUser: null, userType: null })
          set({ isLoading: false })
        }
      },
    }),
    {
      name: "auth-storage", // localStorage key
    },
  ),
)
