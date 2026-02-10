"use client"

import { Download, Edit2 } from "lucide-react"
import { useState } from "react"
import { toast } from "sonner"

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"

import type { UserProfileDto } from "@/lib/api/services/profile.service"
import type { CvFileData } from "@/lib/api/services/cv.service"
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth"
import { useApi } from "@/hooks/useApi"
import { profileService } from "@/lib/api/services/profile.service"
import { downloadFromSasUrl } from "@/lib/fileUtils"
import EditProfileModal from "./EditProfileModal"
import CVUploadSection from "./CVUploadSection"

export default function ProfileDisplaySection({
  profile,
  cvFile = null,
  onRefresh,
}: {
  profile: UserProfileDto
  cvFile?: CvFileData | null // Not fetched - resume download uses /userprofile/candidate/{id}/resume endpoint
  onRefresh: (() => void) | (() => Promise<void>)
}) {
  const [isEditModalOpen, setIsEditModalOpen] = useState(false)
  const [downloadingResume, setDownloadingResume] = useState(false)
  const { getAccessToken } = useUnifiedAuth()
  const api = useApi()

  const handleSaveProfile = async (data: Partial<UserProfileDto>) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      console.log("Saving profile data:", data)
      await profileService.updateUserProfile(token, data)
      console.log("Profile saved successfully")
      toast.success("Profile updated successfully")
      setIsEditModalOpen(false)
      if (typeof onRefresh === "function") {
        await Promise.resolve(onRefresh())
      }
    } catch (error) {
      console.error("Error saving profile:", error)
      toast.error("Failed to save profile")
    }
  }

  const handleDownloadResume = async () => {
    if (!profile.id || downloadingResume) return

    const candidateName = profile.name || "Resume"
    setDownloadingResume(true)
    try {
      // Get SAS URL from backend using the same endpoint as candidate details page
      const response = await api.get(`/userprofile/me/resume`)

      // Response format: { downloadUrl: string, expiresInMinutes: number }
      const downloadUrl = (response as any).downloadUrl || (response as any).data?.downloadUrl

      if (!downloadUrl) {
        console.error("Download response:", response)
        throw new Error("Download URL not received from server")
      }

      // Download directly from Azure using SAS URL
      await downloadFromSasUrl(downloadUrl, `${candidateName}_Resume.pdf`)
    } catch (error) {
      console.error("Failed to download resume:", error)
    } finally {
      setDownloadingResume(false)
    }
  }

  return (
    <>
      <div className="space-y-8">
        <CVUploadSection onRefresh={onRefresh} existingCvFile={cvFile} />

        <Card className="border border-border/40 shadow-xl bg-gradient-to-br from-background via-secondary/5 to-background overflow-hidden hover:shadow-2xl transition-shadow duration-300 dark:from-zinc-950 dark:to-zinc-900">
          <CardHeader className="flex flex-row items-center justify-between pb-8 border-b border-border/40 bg-gradient-to-r from-secondary/5 to-background dark:from-zinc-900 dark:to-zinc-950">
            <div className="space-y-2">
              <CardTitle className="text-4xl font-bold bg-gradient-to-r from-foreground to-foreground/60 bg-clip-text text-transparent dark:from-zinc-50 dark:to-zinc-300">
                Professional Profile
              </CardTitle>
              <CardDescription className="text-base text-muted-foreground dark:text-zinc-400">
                Your personal details and professional preferences
              </CardDescription>
            </div>
            <Button
              onClick={() => setIsEditModalOpen(true)}
              className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 text-primary-foreground shadow-lg hover:shadow-xl transition-all duration-200 dark:from-blue-600 dark:to-blue-700 dark:hover:from-blue-700 dark:hover:to-blue-800"
            >
              <Edit2 className="h-4 w-4 mr-2" />
              Edit Profile
            </Button>
          </CardHeader>

          <CardContent className="space-y-8 pt-8">
            {/* Profile Picture and Name Section */}
            <div className="flex flex-col md:flex-row items-start md:items-center gap-8 pb-8 border-b border-border/40">
              <div className="flex-shrink-0">
                <Avatar className="h-48 w-48 shadow-2xl rounded-2xl ring-4 ring-border/20 hover:ring-border/40 transition-all duration-300 dark:ring-zinc-700 dark:hover:ring-zinc-600">
                  <AvatarImage src={profile.profilePictureUrl || ""} alt={profile.name} />
                  <AvatarFallback className="text-5xl bg-gradient-to-br from-primary to-primary/60 text-primary-foreground font-bold rounded-2xl dark:from-blue-600 dark:to-blue-700">
                    {profile.name?.slice(0, 1).toUpperCase()}
                  </AvatarFallback>
                </Avatar>
              </div>

              <div className="flex-1 space-y-4">
                <div>
                  <h3 className="text-5xl font-bold text-foreground mb-2 dark:text-zinc-50">
                    {profile.name || "Not provided"}
                  </h3>
                  <p className="text-xl text-primary font-semibold mb-4 dark:text-blue-400">
                    {profile.headline || "Professional Profile"}
                  </p>
                  {profile.bio && (
                    <p className="text-foreground/70 leading-relaxed max-w-3xl text-base dark:text-zinc-300">
                      {profile.bio}
                    </p>
                  )}
                </div>
              </div>
            </div>

            {/* Personal Details Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {[
                { label: "Phone Number", value: profile.phoneNumber, icon: "📱" },
                { label: "Age", value: profile.age, icon: "🎂" },
                { label: "Nationality", value: profile.nationality, icon: "🌍" },
              ].map((item) => (
                <div
                  key={item.label}
                  className="p-5 rounded-xl bg-gradient-to-br from-secondary/10 to-secondary/5 dark:from-zinc-800 dark:to-zinc-900 border border-border/40 dark:border-zinc-700/50 hover:border-border/60 dark:hover:border-zinc-600/50 hover:shadow-md transition-all duration-200 group"
                >
                  <div className="flex items-start gap-3">
                    <span className="text-2xl">{item.icon}</span>
                    <div className="flex-1">
                      <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest mb-2 dark:text-zinc-400">
                        {item.label}
                      </p>
                      <p className="text-lg font-semibold text-foreground group-hover:text-primary transition-colors dark:text-zinc-100 dark:group-hover:text-blue-400">
                        {item.value || "Not provided"}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
              <div className="p-5 rounded-xl bg-gradient-to-br from-secondary/10 to-secondary/5 dark:from-zinc-800 dark:to-zinc-900 border border-border/40 dark:border-zinc-700/50 hover:border-border/60 dark:hover:border-zinc-600/50 hover:shadow-md transition-all duration-200 group">
                <div className="flex items-start gap-3">
                  <span className="text-2xl">✈️</span>
                  <div className="flex-1">
                    <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest mb-2 dark:text-zinc-400">
                      Open to Relocation
                    </p>
                    {profile.openToRelocation ? (
                      <Badge className="bg-emerald-100 text-emerald-700 hover:bg-emerald-200 font-semibold dark:bg-emerald-950 dark:text-emerald-300 dark:hover:bg-emerald-900">
                        Yes
                      </Badge>
                    ) : (
                      <Badge className="bg-secondary/20 text-foreground hover:bg-secondary/30 font-semibold dark:bg-zinc-700 dark:text-zinc-200 dark:hover:bg-zinc-600">
                        No
                      </Badge>
                    )}
                  </div>
                </div>
              </div>
            </div>

            {/* Job Type Preferences */}
            <div className="space-y-4 pt-4 border-t border-border/40">
              <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest dark:text-zinc-400">
                Job Type Preferences
              </p>
              <div className="flex flex-wrap gap-3">
                {profile.jobTypePreferences && profile.jobTypePreferences.length > 0 ? (
                  profile.jobTypePreferences.map((pref) => (
                    <Badge
                      key={pref}
                      className="px-4 py-2 bg-gradient-to-r from-primary to-primary/80 text-primary-foreground hover:from-primary/90 hover:to-primary/70 capitalize font-semibold shadow-md transition-all duration-200 dark:from-blue-600 dark:to-blue-700 dark:hover:from-blue-700 dark:hover:to-blue-800"
                    >
                      {pref}
                    </Badge>
                  ))
                ) : (
                  <span className="text-muted-foreground italic dark:text-zinc-400">Not provided</span>
                )}
              </div>
            </div>

            {/* Remote Preferences */}
            <div className="space-y-4">
              <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest dark:text-zinc-400">
                Remote Work Preferences
              </p>
              <div className="flex flex-wrap gap-3">
                {profile.remotePreferences && profile.remotePreferences.length > 0 ? (
                  profile.remotePreferences.map((pref) => (
                    <Badge
                      key={pref}
                      className="px-4 py-2 bg-secondary/20 text-foreground hover:bg-secondary/30 capitalize font-semibold border border-border/40 dark:bg-zinc-700 dark:text-zinc-100 dark:hover:bg-zinc-600 dark:border-zinc-600/50 transition-all duration-200"
                    >
                      {pref}
                    </Badge>
                  ))
                ) : (
                  <span className="text-muted-foreground italic dark:text-zinc-400">Not provided</span>
                )}
              </div>
            </div>

            {/* Resume URL */}
            <div className="pt-4 border-t border-border/40">
              <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest mb-4 dark:text-zinc-400">
                Resume
              </p>
              <Button
                onClick={handleDownloadResume}
                disabled={downloadingResume || !profile.id}
                className="inline-flex items-center px-6 py-3 bg-gradient-to-r from-primary to-primary/80 text-primary-foreground rounded-lg font-semibold hover:from-primary/90 hover:to-primary/70 transition-all duration-200 shadow-lg hover:shadow-xl dark:from-blue-600 dark:to-blue-700 dark:hover:from-blue-700 dark:hover:to-blue-800"
              >
                <Download className="h-4 w-4 mr-2" />
                {downloadingResume ? "Downloading..." : "Download Resume"}
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>

      <EditProfileModal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        profile={profile}
        onSave={handleSaveProfile}
      />
    </>
  )
}
