"use client"

import { Plus, Edit2, Trash2, Heart } from "lucide-react"
import { useState } from "react"
import { toast } from "sonner"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"

import type { VolunteerExtracurricularDto } from "@/lib/api/services/profile.service"
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth"
import { profileService } from "@/lib/api/services/profile.service"
import VolunteerModal from "./VolunteerModal"

export default function VolunteerDisplaySection({
  volunteers,
  userProfileId,
  onRefresh,
}: {
  volunteers: VolunteerExtracurricularDto[]
  userProfileId: string
  onRefresh: () => void
}) {
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingVolunteer, setEditingVolunteer] = useState<VolunteerExtracurricularDto | null>(null)
  const [isDeleting, setIsDeleting] = useState<string | null>(null)
  const { getAccessToken } = useUnifiedAuth()

  const handleAddVolunteer = () => {
    setEditingVolunteer(null)
    setIsModalOpen(true)
  }

  const handleEditVolunteer = (volunteer: VolunteerExtracurricularDto) => {
    setEditingVolunteer(volunteer)
    setIsModalOpen(true)
  }

  const handleDeleteVolunteer = async (id: string) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      setIsDeleting(id)
      await profileService.deleteVolunteerActivity(token, id)
      toast.success("Volunteer activity deleted successfully")
      onRefresh()
    } catch (error) {
      console.error("Error deleting volunteer activity:", error)
      toast.error("Failed to delete volunteer activity")
    } finally {
      setIsDeleting(null)
    }
  }

  const handleSaveVolunteer = async (data: Omit<VolunteerExtracurricularDto, "id" | "createdAt" | "updatedAt">) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      if (editingVolunteer) {
        await profileService.updateVolunteerActivity(token, editingVolunteer.id, data)
        toast.success("Volunteer activity updated successfully")
      } else {
        await profileService.addVolunteerActivity(token, {
          ...data,
          userProfileId,
        })
        toast.success("Volunteer activity added successfully")
      }

      setIsModalOpen(false)
      setEditingVolunteer(null)
      onRefresh()
    } catch (error) {
      console.error("Error saving volunteer activity:", error)
      toast.error("Failed to save volunteer activity")
    }
  }

  return (
    <>
      <Card className="border border-border/40 shadow-xl bg-gradient-to-br from-background via-secondary/5 to-background overflow-hidden hover:shadow-2xl transition-shadow duration-300 dark:from-zinc-950 dark:to-zinc-900">
        <CardHeader className="flex flex-row items-center justify-between pb-8 border-b border-border/40 bg-gradient-to-r from-secondary/5 to-background dark:from-zinc-900 dark:to-zinc-950">
          <div className="space-y-2">
            <CardTitle className="text-3xl font-bold bg-gradient-to-r from-foreground to-foreground/60 bg-clip-text text-transparent dark:from-zinc-50 dark:to-zinc-300">
              Volunteer & Extracurricular
            </CardTitle>
            <CardDescription className="text-base text-muted-foreground dark:text-zinc-400">
              Your volunteer work and extracurricular activities
            </CardDescription>
          </div>
          <Button
            onClick={handleAddVolunteer}
            className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 text-primary-foreground shadow-lg hover:shadow-xl transition-all duration-200 dark:from-blue-600 dark:to-blue-700 dark:hover:from-blue-700 dark:hover:to-blue-800"
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Activity
          </Button>
        </CardHeader>

        <CardContent className="pt-8">
          {volunteers.length === 0 ? (
            <div className="text-center py-16">
              <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gradient-to-br from-secondary/10 to-secondary/5 dark:from-zinc-800 dark:to-zinc-900 mb-4 border border-border/40 dark:border-zinc-700/50">
                <Heart className="h-10 w-10 text-muted-foreground dark:text-zinc-500" />
              </div>
              <p className="text-foreground text-lg font-semibold mt-4 dark:text-zinc-100">
                No volunteer activities added yet
              </p>
              <p className="text-muted-foreground text-sm mt-2 dark:text-zinc-400">
                Add your volunteer work and extracurricular activities
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              {volunteers.map((volunteer) => (
                <Card
                  key={volunteer.id}
                  className="border border-border/40 bg-gradient-to-br from-secondary/5 to-background dark:from-zinc-800 dark:to-zinc-900 hover:shadow-lg transition-all duration-200 hover:border-border/60 dark:hover:border-zinc-600/50 group"
                >
                  <CardContent className="pt-6">
                    <div className="flex gap-4">
                      <div className="flex-shrink-0 w-16 h-16 rounded-xl bg-gradient-to-br from-rose-200/40 to-rose-300/40 dark:from-rose-600 dark:to-rose-700 flex items-center justify-center border border-rose-300/50 dark:border-rose-600/50 shadow-lg group-hover:shadow-xl transition-shadow">
                        <Heart className="h-7 w-7 text-rose-600 dark:text-rose-100" />
                      </div>
                      <div className="flex-1">
                        <div className="flex justify-between items-start gap-4">
                          <div className="flex-1 space-y-3">
                            <h4 className="font-semibold text-lg text-foreground group-hover:text-primary transition-colors dark:text-zinc-100 dark:group-hover:text-blue-400">
                              {volunteer.role || "Not provided"}
                            </h4>
                            <div className="space-y-2">
                              {volunteer.organization && (
                                <div className="p-3 rounded-lg bg-secondary/10 dark:bg-zinc-700/50 border border-border/40 dark:border-zinc-600/50">
                                  <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1 dark:text-zinc-400">
                                    Organization
                                  </p>
                                  <p className="text-sm font-semibold text-foreground dark:text-zinc-100">
                                    {volunteer.organization}
                                  </p>
                                </div>
                              )}
                              {(volunteer.startDate || volunteer.endDate) && (
                                <div className="p-3 rounded-lg bg-secondary/10 dark:bg-zinc-700/50 border border-border/40 dark:border-zinc-600/50">
                                  <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1 dark:text-zinc-400">
                                    Duration
                                  </p>
                                  <p className="text-sm font-semibold text-foreground dark:text-zinc-100">
                                    {volunteer.startDate &&
                                      new Date(volunteer.startDate).toLocaleDateString("en-US", {
                                        year: "numeric",
                                        month: "short",
                                      })}{" "}
                                    {volunteer.endDate && (
                                      <>
                                        -{" "}
                                        {new Date(volunteer.endDate).toLocaleDateString("en-US", {
                                          year: "numeric",
                                          month: "short",
                                        })}
                                      </>
                                    )}
                                  </p>
                                </div>
                              )}
                            </div>
                            {volunteer.description && (
                              <p className="text-foreground/70 mt-4 leading-relaxed dark:text-zinc-300">
                                {volunteer.description}
                              </p>
                            )}
                          </div>
                          <div className="flex gap-2 flex-shrink-0">
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleEditVolunteer(volunteer)}
                              className="hover:bg-secondary/20 text-muted-foreground hover:text-foreground transition-colors dark:hover:bg-zinc-700 dark:text-zinc-400 dark:hover:text-zinc-100"
                            >
                              <Edit2 className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleDeleteVolunteer(volunteer.id)}
                              disabled={isDeleting === volunteer.id}
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

      <VolunteerModal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false)
          setEditingVolunteer(null)
        }}
        volunteer={editingVolunteer}
        onSave={handleSaveVolunteer}
      />
    </>
  )
}
