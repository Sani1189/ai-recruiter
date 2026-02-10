"use client"

import { Plus, Edit2, Trash2, Zap } from "lucide-react"
import { useState } from "react"
import { toast } from "sonner"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"

import type { SkillDto } from "@/lib/api/services/profile.service"
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth"
import { profileService } from "@/lib/api/services/profile.service"
import SkillModal from "./SkillModal"

export default function SkillsDisplaySection({
  skills,
  userProfileId,
  onRefresh,
}: {
  skills: SkillDto[]
  userProfileId: string
  onRefresh: () => void
}) {
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingSkill, setEditingSkill] = useState<SkillDto | null>(null)
  const [isDeleting, setIsDeleting] = useState<string | null>(null)
  const { getAccessToken } = useUnifiedAuth()

  const handleAddSkill = () => {
    setEditingSkill(null)
    setIsModalOpen(true)
  }

  const handleEditSkill = (skill: SkillDto) => {
    setEditingSkill(skill)
    setIsModalOpen(true)
  }

  const handleDeleteSkill = async (id: string) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      setIsDeleting(id)
      console.log("Deleting skill:", id)
      await profileService.deleteSkill(token, id)
      console.log("Skill deleted successfully")
      toast.success("Skill deleted successfully")
      onRefresh()
    } catch (error) {
      console.error("Error deleting skill:", error)
      toast.error("Failed to delete skill")
    } finally {
      setIsDeleting(null)
    }
  }

  const handleSaveSkill = async (data: Omit<SkillDto, "id" | "createdAt" | "updatedAt">) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      console.log("Saving skill:", data)

      if (editingSkill) {
        await profileService.updateSkill(token, editingSkill.id, data)
        console.log("Skill updated successfully")
        toast.success("Skill updated successfully")
      } else {
        await profileService.addSkill(token, {
          ...data,
          userProfileId,
        })
        console.log("Skill added successfully")
        toast.success("Skill added successfully")
      }

      setIsModalOpen(false)
      setEditingSkill(null)
      onRefresh()
    } catch (error) {
      console.error("Error saving skill:", error)
      toast.error("Failed to save skill")
    }
  }

  return (
    <>
      <Card className="border border-border/40 shadow-xl bg-gradient-to-br from-background via-secondary/5 to-background overflow-hidden hover:shadow-2xl transition-shadow duration-300 dark:from-zinc-950 dark:to-zinc-900">
        <CardHeader className="flex flex-row items-center justify-between pb-8 border-b border-border/40 bg-gradient-to-r from-secondary/5 to-background dark:from-zinc-900 dark:to-zinc-950">
          <div className="space-y-2">
            <CardTitle className="text-3xl font-bold text-foreground dark:text-zinc-50">Skills</CardTitle>
            <CardDescription className="text-base text-muted-foreground dark:text-zinc-400">
              Your technical and professional competencies
            </CardDescription>
          </div>
          <Button
            onClick={handleAddSkill}
            className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 text-primary-foreground shadow-lg hover:shadow-xl transition-all duration-200 dark:from-blue-600 dark:to-blue-700 dark:hover:from-blue-700 dark:hover:to-blue-800"
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Skill
          </Button>
        </CardHeader>

        <CardContent className="pt-8">
          {skills.length === 0 ? (
            <div className="text-center py-16">
              <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gradient-to-br from-secondary/10 to-secondary/5 dark:from-zinc-800 dark:to-zinc-900 mb-4 border border-border/40 dark:border-zinc-700/50">
                <Zap className="h-10 w-10 text-muted-foreground dark:text-zinc-500" />
              </div>
              <p className="text-foreground text-lg font-semibold mt-4 dark:text-zinc-100">No skills added yet</p>
              <p className="text-muted-foreground text-sm mt-2 dark:text-zinc-400">
                Add your professional skills to showcase your expertise
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {skills.map((skill) => (
                <Card
                  key={skill.id}
                  className="border border-border/40 bg-gradient-to-br from-secondary/5 to-background dark:from-zinc-800 dark:to-zinc-900 hover:shadow-lg transition-all duration-200 hover:border-border/60 dark:hover:border-zinc-600/50 group"
                >
                  <CardContent className="pt-6">
                    <div className="flex justify-between items-start gap-4">
                      <div className="flex-1 space-y-4">
                        <div className="flex items-start gap-3">
                          <div className="flex-shrink-0 w-10 h-10 rounded-lg bg-gradient-to-br from-primary/20 to-primary/40 dark:from-blue-600/30 dark:to-blue-700/30 flex items-center justify-center border border-primary/30 dark:border-blue-600/50">
                            <Zap className="h-5 w-5 text-primary dark:text-blue-400" />
                          </div>
                          <h4 className="font-bold text-lg text-foreground group-hover:text-primary transition-colors dark:text-zinc-100 dark:group-hover:text-blue-400">
                            {skill.skillName || "Not provided"}
                          </h4>
                        </div>
                        <div className="space-y-2">
                          {skill.category && (
                            <div className="p-3 rounded-lg bg-secondary/10 dark:bg-zinc-700/50 border border-border/40 dark:border-zinc-600/50">
                              <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1 dark:text-zinc-400">
                                Category
                              </p>
                              <p className="text-sm font-semibold text-foreground dark:text-zinc-100">
                                {skill.category}
                              </p>
                            </div>
                          )}
                          {skill.proficiency && (
                            <div className="p-3 rounded-lg bg-secondary/10 dark:bg-zinc-700/50 border border-border/40 dark:border-zinc-600/50">
                              <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1 dark:text-zinc-400">
                                Proficiency
                              </p>
                              <Badge className="bg-secondary/20 text-foreground capitalize font-semibold dark:bg-zinc-600 dark:text-zinc-100">
                                {skill.proficiency}
                              </Badge>
                            </div>
                          )}
                          {skill.yearsExperience && (
                            <div className="p-3 rounded-lg bg-secondary/10 dark:bg-zinc-700/50 border border-border/40 dark:border-zinc-600/50">
                              <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1 dark:text-zinc-400">
                                Experience
                              </p>
                              <p className="text-sm font-semibold text-foreground dark:text-zinc-100">
                                {skill.yearsExperience} years
                              </p>
                            </div>
                          )}
                        </div>
                      </div>
                      <div className="flex gap-2 flex-shrink-0">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleEditSkill(skill)}
                          className="hover:bg-secondary/20 text-muted-foreground hover:text-foreground transition-colors dark:hover:bg-zinc-700 dark:text-zinc-400 dark:hover:text-zinc-100"
                        >
                          <Edit2 className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDeleteSkill(skill.id)}
                          disabled={isDeleting === skill.id}
                          className="hover:bg-red-50 hover:text-red-600 text-muted-foreground transition-colors dark:hover:bg-red-950 dark:hover:text-red-400 dark:text-zinc-400"
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <SkillModal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false)
          setEditingSkill(null)
        }}
        skill={editingSkill}
        onSave={handleSaveSkill}
      />
    </>
  )
}
