"use client"

import { Plus, Edit2, Trash2, Zap } from "lucide-react"
import { useState } from "react"
import { toast } from "sonner"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"

import type { KeyStrengthDto } from "@/lib/api/services/profile.service"
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth"
import { profileService } from "@/lib/api/services/profile.service"
import KeyStrengthModal from "./KeyStrengthModal"

export default function KeyStrengthDisplaySection({
  strengths,
  userProfileId,
  onRefresh,
}: {
  strengths: KeyStrengthDto[]
  userProfileId: string
  onRefresh: () => void
}) {
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingStrength, setEditingStrength] = useState<KeyStrengthDto | null>(null)
  const [isDeleting, setIsDeleting] = useState<string | null>(null)
  const { getAccessToken } = useUnifiedAuth()

  const handleAddStrength = () => {
    setEditingStrength(null)
    setIsModalOpen(true)
  }

  const handleEditStrength = (strength: KeyStrengthDto) => {
    setEditingStrength(strength)
    setIsModalOpen(true)
  }

  const handleDeleteStrength = async (id: string) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      setIsDeleting(id)
      await profileService.deleteKeyStrength(token, id)
      toast.success("Strength deleted successfully")
      onRefresh()
    } catch (error) {
      console.error("Error deleting key strength:", error)
      toast.error("Failed to delete strength")
    } finally {
      setIsDeleting(null)
    }
  }

  const handleSaveStrength = async (data: Omit<KeyStrengthDto, "id" | "createdAt" | "updatedAt">) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      if (editingStrength) {
        await profileService.updateKeyStrength(token, editingStrength.id, data)
        toast.success("Strength updated successfully")
      } else {
        await profileService.addKeyStrength(token, {
          ...data,
          userProfileId,
        })
        toast.success("Strength added successfully")
      }

      setIsModalOpen(false)
      setEditingStrength(null)
      onRefresh()
    } catch (error) {
      console.error("Error saving key strength:", error)
      toast.error("Failed to save strength")
    }
  }

  return (
    <>
      <Card className="border border-border/40 shadow-xl bg-gradient-to-br from-background via-secondary/5 to-background overflow-hidden hover:shadow-2xl transition-shadow duration-300 dark:from-zinc-950 dark:to-zinc-900">
        <CardHeader className="flex flex-row items-center justify-between pb-8 border-b border-border/40 bg-gradient-to-r from-secondary/5 to-background dark:from-zinc-900 dark:to-zinc-950">
          <div className="space-y-2">
            <CardTitle className="text-3xl font-bold bg-gradient-to-r from-foreground to-foreground/60 bg-clip-text text-transparent dark:from-zinc-50 dark:to-zinc-300">
              Key Strengths
            </CardTitle>
            <CardDescription className="text-base text-muted-foreground dark:text-zinc-400">
              Your core competencies and professional strengths
            </CardDescription>
          </div>
          <Button
            onClick={handleAddStrength}
            className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 text-primary-foreground shadow-lg hover:shadow-xl transition-all duration-200 dark:from-blue-600 dark:to-blue-700 dark:hover:from-blue-700 dark:hover:to-blue-800"
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Strength
          </Button>
        </CardHeader>

        <CardContent className="pt-8">
          {strengths.length === 0 ? (
            <div className="text-center py-16">
              <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gradient-to-br from-secondary/10 to-secondary/5 dark:from-zinc-800 dark:to-zinc-900 mb-4 border border-border/40 dark:border-zinc-700/50">
                <Zap className="h-10 w-10 text-muted-foreground dark:text-zinc-500" />
              </div>
              <p className="text-foreground text-lg font-semibold mt-4 dark:text-zinc-100">No strengths added yet</p>
              <p className="text-muted-foreground text-sm mt-2 dark:text-zinc-400">
                Add your key strengths to highlight your professional competencies
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {strengths.map((strength) => (
                <Card
                  key={strength.id}
                  className="border border-border/40 bg-gradient-to-br from-secondary/5 to-background dark:from-zinc-800 dark:to-zinc-900 hover:shadow-lg transition-all duration-200 hover:border-border/60 dark:hover:border-zinc-600/50 group"
                >
                  <CardContent className="pt-6">
                    <div className="flex justify-between items-start gap-4">
                      <div className="flex-1 space-y-4">
                        <div className="flex items-start gap-3">
                          <div className="flex-shrink-0 w-10 h-10 rounded-lg bg-gradient-to-br from-purple-200/40 to-purple-300/40 dark:from-purple-600/30 dark:to-purple-700/30 flex items-center justify-center border border-purple-300/50 dark:border-purple-600/50">
                            <Zap className="h-5 w-5 text-purple-600 dark:text-purple-400" />
                          </div>
                          <h4 className="font-bold text-lg text-foreground group-hover:text-primary transition-colors dark:text-zinc-100 dark:group-hover:text-blue-400">
                            {strength.strengthName || "Not provided"}
                          </h4>
                        </div>
                        {strength.description && (
                          <p className="text-foreground/70 leading-relaxed text-base dark:text-zinc-300">
                            {strength.description}
                          </p>
                        )}
                      </div>
                      <div className="flex gap-2 flex-shrink-0">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleEditStrength(strength)}
                          className="hover:bg-secondary/20 text-muted-foreground hover:text-foreground transition-colors dark:hover:bg-zinc-700 dark:text-zinc-400 dark:hover:text-zinc-100"
                        >
                          <Edit2 className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDeleteStrength(strength.id)}
                          disabled={isDeleting === strength.id}
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

      <KeyStrengthModal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false)
          setEditingStrength(null)
        }}
        strength={editingStrength}
        onSave={handleSaveStrength}
      />
    </>
  )
}
