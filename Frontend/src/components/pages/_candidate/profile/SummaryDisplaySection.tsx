"use client"

import { Plus, Edit2, Trash2, FileText } from "lucide-react"
import { useState } from "react"
import { toast } from "sonner"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"

import type { SummaryDto } from "@/lib/api/services/profile.service"
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth"
import { profileService } from "@/lib/api/services/profile.service"
import SummaryModal from "./SummaryModal"

export default function SummaryDisplaySection({
  summaries,
  userProfileId,
  onRefresh,
}: {
  summaries: SummaryDto[]
  userProfileId: string
  onRefresh: () => void
}) {
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingSummary, setEditingSummary] = useState<SummaryDto | null>(null)
  const [isDeleting, setIsDeleting] = useState<string | null>(null)
  const { getAccessToken } = useUnifiedAuth()

  const handleAddSummary = () => {
    setEditingSummary(null)
    setIsModalOpen(true)
  }

  const handleEditSummary = (summary: SummaryDto) => {
    setEditingSummary(summary)
    setIsModalOpen(true)
  }

  const handleDeleteSummary = async (id: string) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      setIsDeleting(id)
      console.log("Deleting summary:", id)
      await profileService.deleteSummary(token, id)
      console.log("Summary deleted successfully")
      toast.success("Summary deleted successfully")
      onRefresh()
    } catch (error) {
      console.error("Error deleting summary:", error)
      toast.error("Failed to delete summary")
    } finally {
      setIsDeleting(null)
    }
  }

  const handleSaveSummary = async (data: Omit<SummaryDto, "id" | "createdAt" | "updatedAt">) => {
    try {
      const token = await getAccessToken()
      if (!token) {
        toast.error("Authentication error")
        return
      }

      console.log("Saving summary:", data)

      if (editingSummary) {
        await profileService.updateSummary(token, editingSummary.id, data)
        console.log("Summary updated successfully")
        toast.success("Summary updated successfully")
      } else {
        await profileService.addSummary(token, {
          ...data,
          userProfileId,
        })
        console.log("Summary added successfully")
        toast.success("Summary added successfully")
      }

      setIsModalOpen(false)
      setEditingSummary(null)
      onRefresh()
    } catch (error) {
      console.error("Error saving summary:", error)
      toast.error("Failed to save summary")
    }
  }

  return (
    <>
      <Card className="border border-border/40 shadow-xl bg-gradient-to-br from-background via-secondary/5 to-background overflow-hidden hover:shadow-2xl transition-shadow duration-300 dark:from-zinc-950 dark:to-zinc-900">
        <CardHeader className="flex flex-row items-center justify-between pb-8 border-b border-border/40 bg-gradient-to-r from-secondary/5 to-background dark:from-zinc-900 dark:to-zinc-950">
          <div className="space-y-2">
            <CardTitle className="text-3xl font-bold text-foreground dark:text-zinc-50">Professional Summary</CardTitle>
            <CardDescription className="text-base text-muted-foreground dark:text-zinc-400">
              Your career objectives and professional overview
            </CardDescription>
          </div>
          <Button
            onClick={handleAddSummary}
            className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 text-primary-foreground shadow-lg hover:shadow-xl transition-all duration-200 dark:from-blue-600 dark:to-blue-700 dark:hover:from-blue-700 dark:hover:to-blue-800"
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Summary
          </Button>
        </CardHeader>

        <CardContent className="pt-8">
          {summaries.length === 0 ? (
            <div className="text-center py-16">
              <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gradient-to-br from-secondary/10 to-secondary/5 dark:from-zinc-800 dark:to-zinc-900 mb-4 border border-border/40 dark:border-zinc-700/50">
                <FileText className="h-10 w-10 text-muted-foreground dark:text-zinc-500" />
              </div>
              <p className="text-foreground text-lg font-semibold mt-4 dark:text-zinc-100">No summary added yet</p>
              <p className="text-muted-foreground text-sm mt-2 dark:text-zinc-400">
                Add a professional summary to highlight your career objectives
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              {summaries.map((summary) => (
                <Card
                  key={summary.id}
                  className="border border-border/40 bg-gradient-to-br from-secondary/5 to-background dark:from-zinc-800 dark:to-zinc-900 hover:shadow-lg transition-all duration-200 hover:border-border/60 dark:hover:border-zinc-600/50 group"
                >
                  <CardContent className="pt-6">
                    <div className="flex justify-between items-start gap-4">
                      <div className="flex-1 space-y-3">
                        {summary.type && (
                          <h4 className="font-bold text-lg text-foreground group-hover:text-primary transition-colors dark:text-zinc-100 dark:group-hover:text-blue-400">
                            {summary.type}
                          </h4>
                        )}
                        {summary.text && (
                          <p className="text-foreground/70 leading-relaxed whitespace-pre-wrap text-base dark:text-zinc-300">
                            {summary.text}
                          </p>
                        )}
                      </div>
                      <div className="flex gap-2 flex-shrink-0">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleEditSummary(summary)}
                          className="hover:bg-secondary/20 text-muted-foreground hover:text-foreground transition-colors dark:hover:bg-zinc-700 dark:text-zinc-400 dark:hover:text-zinc-100"
                        >
                          <Edit2 className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDeleteSummary(summary.id)}
                          disabled={isDeleting === summary.id}
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

      <SummaryModal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false)
          setEditingSummary(null)
        }}
        summary={editingSummary}
        onSave={handleSaveSummary}
      />
    </>
  )
}
