"use client"

import { Plus, Edit2, Trash2, Award } from "lucide-react"
import { useState } from "react"
import { toast } from "sonner"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"

import type { AwardAchievementDto } from "@/lib/api/services/profile.service"
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth"
import { profileService } from "@/lib/api/services/profile.service"
import AwardAchievementModal from "./AwardAchievementModal"

export default function AwardAchievementDisplaySection({
  awards,
  userProfileId,
  onRefresh,
}: {
  awards: AwardAchievementDto[]
  userProfileId: string
  onRefresh: () => void
}) {
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingAward, setEditingAward] = useState<AwardAchievementDto | null>(null)
  const [isDeleting, setIsDeleting] = useState<string | null>(null)
  const { getAccessToken } = useUnifiedAuth()

  const handleAddAward = () => {
    setEditingAward(null)
    setIsModalOpen(true)
  }

  const handleEditAward = (award: AwardAchievementDto) => {
    setEditingAward(award)
    setIsModalOpen(true)
  }

  const handleDeleteAward = async (id: string) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      setIsDeleting(id)
      await profileService.deleteAwardAchievement(token, id)
      toast.success("Award deleted successfully")
      onRefresh()
    } catch (error) {
      console.error("Error deleting award achievement:", error)
      toast.error("Failed to delete award")
    } finally {
      setIsDeleting(null)
    }
  }

  const handleSaveAward = async (data: Omit<AwardAchievementDto, "id" | "createdAt" | "updatedAt">) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      if (editingAward) {
        await profileService.updateAwardAchievement(token, editingAward.id, data)
        toast.success("Award updated successfully")
      } else {
        await profileService.addAwardAchievement(token, {
          ...data,
          userProfileId,
        })
        toast.success("Award added successfully")
      }

      setIsModalOpen(false)
      setEditingAward(null)
      onRefresh()
    } catch (error) {
      console.error("Error saving award achievement:", error)
      toast.error("Failed to save award")
    }
  }

  return (
    <>
      <Card className="border border-border/40 shadow-xl bg-gradient-to-br from-background via-secondary/5 to-background overflow-hidden hover:shadow-2xl transition-shadow duration-300 dark:from-zinc-950 dark:to-zinc-900">
        <CardHeader className="flex flex-row items-center justify-between pb-8 border-b border-border/40 bg-gradient-to-r from-secondary/5 to-background dark:from-zinc-900 dark:to-zinc-950">
          <div className="space-y-2">
            <CardTitle className="text-3xl font-bold bg-gradient-to-r from-foreground to-foreground/60 bg-clip-text text-transparent dark:from-zinc-50 dark:to-zinc-300">
              Awards & Achievements
            </CardTitle>
            <CardDescription className="text-base text-muted-foreground dark:text-zinc-400">
              Your awards, recognitions, and notable achievements
            </CardDescription>
          </div>
          <Button
            onClick={handleAddAward}
            className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 text-primary-foreground shadow-lg hover:shadow-xl transition-all duration-200 dark:from-blue-600 dark:to-blue-700 dark:hover:from-blue-700 dark:hover:to-blue-800"
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Award
          </Button>
        </CardHeader>

        <CardContent className="pt-8">
          {awards.length === 0 ? (
            <div className="text-center py-16">
              <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gradient-to-br from-secondary/10 to-secondary/5 dark:from-zinc-800 dark:to-zinc-900 mb-4 border border-border/40 dark:border-zinc-700/50">
                <Award className="h-10 w-10 text-muted-foreground dark:text-zinc-500" />
              </div>
              <p className="text-foreground text-lg font-semibold mt-4 dark:text-zinc-100">No awards added yet</p>
              <p className="text-muted-foreground text-sm mt-2 dark:text-zinc-400">
                Add your awards and achievements to highlight your accomplishments
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              {awards.map((award) => (
                <Card
                  key={award.id}
                  className="border border-border/40 bg-gradient-to-br from-secondary/5 to-background dark:from-zinc-800 dark:to-zinc-900 hover:shadow-lg transition-all duration-200 hover:border-border/60 dark:hover:border-zinc-600/50 group"
                >
                  <CardContent className="pt-6">
                    <div className="flex gap-4">
                      <div className="flex-shrink-0 w-16 h-16 rounded-xl bg-gradient-to-br from-amber-200/40 to-amber-300/40 dark:from-amber-600 dark:to-amber-700 flex items-center justify-center border border-amber-300/50 dark:border-amber-600/50 shadow-lg group-hover:shadow-xl transition-shadow">
                        <Award className="h-7 w-7 text-amber-600 dark:text-amber-100" />
                      </div>
                      <div className="flex-1">
                        <div className="flex justify-between items-start gap-4">
                          <div className="flex-1 space-y-3">
                            <h4 className="font-bold text-lg text-foreground group-hover:text-primary transition-colors dark:text-zinc-100 dark:group-hover:text-blue-400">
                              {award.title || "Not provided"}
                            </h4>
                            <div className="space-y-2">
                              {award.issuer && (
                                <div className="p-3 rounded-lg bg-secondary/10 dark:bg-zinc-700/50 border border-border/40 dark:border-zinc-600/50">
                                  <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1 dark:text-zinc-400">
                                    Issuer
                                  </p>
                                  <p className="text-sm font-semibold text-foreground dark:text-zinc-100">
                                    {award.issuer}
                                  </p>
                                </div>
                              )}
                              {award.year && (
                                <div className="p-3 rounded-lg bg-secondary/10 dark:bg-zinc-700/50 border border-border/40 dark:border-zinc-600/50">
                                  <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1 dark:text-zinc-400">
                                    Year
                                  </p>
                                  <p className="text-sm font-semibold text-foreground dark:text-zinc-100">
                                    {award.year}
                                  </p>
                                </div>
                              )}
                            </div>
                            {award.description && (
                              <p className="text-foreground/70 mt-4 leading-relaxed text-base dark:text-zinc-300">
                                {award.description}
                              </p>
                            )}
                          </div>
                          <div className="flex gap-2 flex-shrink-0">
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleEditAward(award)}
                              className="hover:bg-secondary/20 text-muted-foreground hover:text-foreground transition-colors dark:hover:bg-zinc-700 dark:text-zinc-400 dark:hover:text-zinc-100"
                            >
                              <Edit2 className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleDeleteAward(award.id)}
                              disabled={isDeleting === award.id}
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

      <AwardAchievementModal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false)
          setEditingAward(null)
        }}
        award={editingAward}
        onSave={handleSaveAward}
      />
    </>
  )
}
