"use client"

import { Plus, Edit2, Trash2, BookOpen, Calendar } from "lucide-react"
import { useState } from "react"
import { toast } from "sonner"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"

import type { EducationDto } from "@/lib/api/services/profile.service"
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth"
import { profileService } from "@/lib/api/services/profile.service"
import EducationModal from "./EducationModal"

export default function EducationDisplaySection({
  education,
  userProfileId,
  onRefresh,
}: {
  education: EducationDto[]
  userProfileId: string
  onRefresh: () => void
}) {
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingEducation, setEditingEducation] = useState<EducationDto | null>(null)
  const [isDeleting, setIsDeleting] = useState<string | null>(null)
  const { getAccessToken } = useUnifiedAuth()

  const handleAddEducation = () => {
    setEditingEducation(null)
    setIsModalOpen(true)
  }

  const handleEditEducation = (edu: EducationDto) => {
    setEditingEducation(edu)
    setIsModalOpen(true)
  }

  const handleDeleteEducation = async (id: string) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      setIsDeleting(id)
      await profileService.deleteEducation(token, id)
      toast.success("Education deleted successfully")
      onRefresh()
    } catch (error) {
      console.error("Error deleting education:", error)
      toast.error("Failed to delete education")
    } finally {
      setIsDeleting(null)
    }
  }

  const handleSaveEducation = async (data: Omit<EducationDto, "id" | "createdAt" | "updatedAt">) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      if (editingEducation) {
        await profileService.updateEducation(token, editingEducation.id, data)
        toast.success("Education updated successfully")
      } else {
        await profileService.addEducation(token, {
          ...data,
          userProfileId,
        })
        toast.success("Education added successfully")
      }

      setIsModalOpen(false)
      setEditingEducation(null)
      onRefresh()
    } catch (error) {
      console.error("Error saving education:", error)
      toast.error("Failed to save education")
    }
  }

  return (
    <>
      <Card className="border border-border/40 shadow-xl bg-gradient-to-br from-background via-secondary/5 to-background overflow-hidden hover:shadow-2xl transition-shadow duration-300 dark:from-zinc-950 dark:to-zinc-900">
        <CardHeader className="flex flex-row items-center justify-between pb-8 border-b border-border/40 bg-gradient-to-r from-secondary/5 to-background dark:from-zinc-900 dark:to-zinc-950">
          <div className="space-y-2">
            <CardTitle className="text-3xl font-bold bg-gradient-to-r from-foreground to-foreground/60 bg-clip-text text-transparent dark:from-zinc-50 dark:to-zinc-300">
              Education
            </CardTitle>
            <CardDescription className="text-base text-muted-foreground dark:text-zinc-400">
              Your educational background and qualifications
            </CardDescription>
          </div>
          <Button
            onClick={handleAddEducation}
            className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 text-primary-foreground shadow-lg hover:shadow-xl transition-all duration-200 dark:from-blue-600 dark:to-blue-700 dark:hover:from-blue-700 dark:hover:to-blue-800"
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Education
          </Button>
        </CardHeader>

        <CardContent className="pt-8">
          {education.length === 0 ? (
            <div className="text-center py-16">
              <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gradient-to-br from-secondary/10 to-secondary/5 dark:from-zinc-800 dark:to-zinc-900 mb-4 border border-border/40 dark:border-zinc-700/50">
                <BookOpen className="h-10 w-10 text-muted-foreground dark:text-zinc-500" />
              </div>
              <p className="text-foreground text-lg font-semibold mt-4 dark:text-zinc-100">
                No education records added yet
              </p>
              <p className="text-muted-foreground text-sm mt-2 dark:text-zinc-400">
                Add your educational qualifications to showcase your academic background
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              {education.map((edu) => (
                <Card
                  key={edu.id}
                  className="border border-border/40 bg-gradient-to-br from-secondary/5 to-background dark:from-zinc-800 dark:to-zinc-900 hover:shadow-lg transition-all duration-200 hover:border-border/60 dark:hover:border-zinc-600/50 group"
                >
                  <CardContent className="pt-6">
                    <div className="flex gap-4">
                      <div className="flex-shrink-0 w-16 h-16 rounded-xl bg-gradient-to-br from-primary/20 to-primary/40 dark:from-blue-600 dark:to-blue-700 flex items-center justify-center border border-primary/30 dark:border-blue-600/50 shadow-lg group-hover:shadow-xl transition-shadow">
                        <BookOpen className="h-7 w-7 text-primary dark:text-blue-100" />
                      </div>
                      <div className="flex-1">
                        <div className="flex justify-between items-start gap-4">
                          <div className="flex-1 space-y-3">
                            <h4 className="font-bold text-lg text-foreground group-hover:text-primary transition-colors dark:text-zinc-100 dark:group-hover:text-blue-400">
                              {edu.degree || "Not provided"}
                            </h4>
                            <p className="text-foreground/70 font-semibold dark:text-zinc-300">
                              {edu.institution || "Not provided"}
                            </p>
                            <div className="flex flex-wrap gap-4 text-sm text-muted-foreground dark:text-zinc-400">
                              {edu.fieldOfStudy && (
                                <span className="inline-flex items-center gap-1 font-medium">
                                  <span className="text-muted-foreground dark:text-zinc-500">•</span>
                                  {edu.fieldOfStudy}
                                </span>
                              )}
                              {edu.endDate && (
                                <span className="inline-flex items-center gap-1">
                                  <Calendar className="h-4 w-4 text-muted-foreground dark:text-zinc-500" />
                                  {new Date(edu.endDate).toLocaleDateString("en-US", {
                                    year: "numeric",
                                    month: "short",
                                  })}
                                </span>
                              )}
                            </div>
                          </div>
                          <div className="flex gap-2 flex-shrink-0">
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleEditEducation(edu)}
                              className="hover:bg-secondary/20 text-muted-foreground hover:text-foreground transition-colors dark:hover:bg-zinc-700 dark:text-zinc-400 dark:hover:text-zinc-100"
                            >
                              <Edit2 className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleDeleteEducation(edu.id)}
                              disabled={isDeleting === edu.id}
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
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <EducationModal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false)
          setEditingEducation(null)
        }}
        education={editingEducation}
        onSave={handleSaveEducation}
      />
    </>
  )
}
