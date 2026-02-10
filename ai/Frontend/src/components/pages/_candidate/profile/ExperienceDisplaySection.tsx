"use client"

import { Plus, Edit2, Trash2, Briefcase, Calendar, MapPin } from "lucide-react"
import { useState } from "react"
import { toast } from "sonner"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"

import type { ExperienceDto } from "@/lib/api/services/profile.service"
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth"
import { profileService } from "@/lib/api/services/profile.service"
import ExperienceModal from "./ExperienceModal"

export default function ExperienceDisplaySection({
  experience,
  userProfileId,
  onRefresh,
}: {
  experience: ExperienceDto[]
  userProfileId: string
  onRefresh: () => void
}) {
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingExperience, setEditingExperience] = useState<ExperienceDto | null>(null)
  const [isDeleting, setIsDeleting] = useState<string | null>(null)
  const { getAccessToken } = useUnifiedAuth()

  const handleAddExperience = () => {
    setEditingExperience(null)
    setIsModalOpen(true)
  }

  const handleEditExperience = (exp: ExperienceDto) => {
    setEditingExperience(exp)
    setIsModalOpen(true)
  }

  const handleDeleteExperience = async (id: string) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      setIsDeleting(id)
      console.log("Deleting experience:", id)
      await profileService.deleteExperience(token, id)
      console.log("Experience deleted successfully")
      toast.success("Experience deleted successfully")
      onRefresh()
    } catch (error) {
      console.error("Error deleting experience:", error)
      toast.error("Failed to delete experience")
    } finally {
      setIsDeleting(null)
    }
  }

  const handleSaveExperience = async (data: Omit<ExperienceDto, "id" | "createdAt" | "updatedAt">) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      console.log("Saving experience:", data)

      if (editingExperience) {
        await profileService.updateExperience(token, editingExperience.id, data)
        console.log("Experience updated successfully")
        toast.success("Experience updated successfully")
      } else {
        await profileService.addExperience(token, {
          ...data,
          userProfileId,
        })
        console.log("Experience added successfully")
        toast.success("Experience added successfully")
      }

      setIsModalOpen(false)
      setEditingExperience(null)
      onRefresh()
    } catch (error) {
      console.error("Error saving experience:", error)
      toast.error("Failed to save experience")
    }
  }

  return (
    <>
      <Card className="border border-border/40 shadow-xl bg-gradient-to-br from-background via-secondary/5 to-background overflow-hidden hover:shadow-2xl transition-shadow duration-300 dark:from-zinc-950 dark:to-zinc-900">
        <CardHeader className="flex flex-row items-center justify-between pb-8 border-b border-border/40 bg-gradient-to-r from-secondary/5 to-background dark:from-zinc-900 dark:to-zinc-950">
          <div className="space-y-2">
            <CardTitle className="text-3xl font-bold text-foreground dark:text-zinc-50">Work Experience</CardTitle>
            <CardDescription className="text-base text-muted-foreground dark:text-zinc-400">
              Your professional work history and achievements
            </CardDescription>
          </div>
          <Button
            onClick={handleAddExperience}
            className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 text-primary-foreground shadow-lg hover:shadow-xl transition-all duration-200 dark:from-blue-600 dark:to-blue-700 dark:hover:from-blue-700 dark:hover:to-blue-800"
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Experience
          </Button>
        </CardHeader>

        <CardContent className="pt-8">
          {experience.length === 0 ? (
            <div className="text-center py-16">
              <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gradient-to-br from-secondary/10 to-secondary/5 dark:from-zinc-800 dark:to-zinc-900 mb-4 border border-border/40 dark:border-zinc-700/50">
                <Briefcase className="h-10 w-10 text-muted-foreground dark:text-zinc-500" />
              </div>
              <p className="text-foreground text-lg font-semibold mt-4 dark:text-zinc-100">
                No work experience added yet
              </p>
              <p className="text-muted-foreground text-sm mt-2 dark:text-zinc-400">
                Add your professional experience to showcase your career
              </p>
            </div>
          ) : (
            <div className="space-y-6">
              {experience.map((exp, index) => (
                <div key={exp.id} className="relative">
                  {index !== experience.length - 1 && (
                    <div className="absolute left-8 top-20 w-0.5 h-12 bg-gradient-to-b from-border/40 to-border/20 dark:from-zinc-600/40 dark:to-zinc-700/20"></div>
                  )}
                  <Card className="border border-border/40 bg-gradient-to-br from-secondary/5 to-background dark:from-zinc-800 dark:to-zinc-900 hover:shadow-lg transition-all duration-200 hover:border-border/60 dark:hover:border-zinc-600/50 group">
                    <CardContent className="pt-6">
                      <div className="flex gap-4">
                        <div className="flex-shrink-0 w-16 h-16 rounded-xl bg-gradient-to-br from-primary/20 to-primary/40 dark:from-blue-600 dark:to-blue-700 flex items-center justify-center border border-primary/30 dark:border-blue-600/50 shadow-lg group-hover:shadow-xl transition-shadow">
                          <Briefcase className="h-7 w-7 text-primary dark:text-blue-100" />
                        </div>
                        <div className="flex-1">
                          <div className="flex justify-between items-start gap-4">
                            <div className="flex-1 space-y-3">
                              <h4 className="font-bold text-lg text-foreground group-hover:text-primary transition-colors dark:text-zinc-100 dark:group-hover:text-blue-400">
                                {exp.title || "Not provided"}
                              </h4>
                              <p className="text-foreground/70 font-semibold dark:text-zinc-300">
                                {exp.organization || "Not provided"}
                              </p>
                              <div className="flex flex-wrap gap-4 text-sm text-muted-foreground dark:text-zinc-400">
                                {exp.location && (
                                  <span className="inline-flex items-center gap-1">
                                    <MapPin className="h-4 w-4 text-muted-foreground dark:text-zinc-500" />
                                    {exp.location}
                                  </span>
                                )}
                                {exp.industry && (
                                  <span className="inline-flex items-center gap-1 font-medium">
                                    <span className="text-muted-foreground dark:text-zinc-500">•</span>
                                    {exp.industry}
                                  </span>
                                )}
                                {exp.startDate && exp.endDate && (
                                  <span className="inline-flex items-center gap-1">
                                    <Calendar className="h-4 w-4 text-muted-foreground dark:text-zinc-500" />
                                    {new Date(exp.startDate).toLocaleDateString("en-US", {
                                      year: "numeric",
                                      month: "short",
                                    })}{" "}
                                    -{" "}
                                    {new Date(exp.endDate).toLocaleDateString("en-US", {
                                      year: "numeric",
                                      month: "short",
                                    })}
                                  </span>
                                )}
                              </div>
                              {exp.description && (
                                <p className="text-foreground/70 mt-4 leading-relaxed text-base dark:text-zinc-300">
                                  {exp.description}
                                </p>
                              )}
                            </div>
                            <div className="flex gap-2 flex-shrink-0">
                              <Button
                                variant="ghost"
                                size="sm"
                                onClick={() => handleEditExperience(exp)}
                                className="hover:bg-secondary/20 text-muted-foreground hover:text-foreground transition-colors dark:hover:bg-zinc-700 dark:text-zinc-400 dark:hover:text-zinc-100"
                              >
                                <Edit2 className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="sm"
                                onClick={() => handleDeleteExperience(exp.id)}
                                disabled={isDeleting === exp.id}
                                className="hover:bg-red-50 hover:text-red-600 text-muted-foreground transition-colors dark:hover:bg-red-950 dark:hover:text-red-400 dark:text-zinc-400"
                              >
                                <Trash2 className="h-4 w-4" />
                              </Button>
                            </div>
                          </div>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <ExperienceModal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false)
          setEditingExperience(null)
        }}
        experience={editingExperience}
        onSave={handleSaveExperience}
      />
    </>
  )
}
