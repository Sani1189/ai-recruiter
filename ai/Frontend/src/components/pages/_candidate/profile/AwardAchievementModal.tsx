"use client"

import { useState, useEffect } from "react"
import { Loader2 } from "lucide-react"

import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"

import type { AwardAchievementDto } from "@/lib/api/services/profile.service"

interface AwardAchievementModalProps {
  isOpen: boolean
  onClose: () => void
  award: AwardAchievementDto | null
  onSave: (data: Omit<AwardAchievementDto, "id" | "createdAt" | "updatedAt">) => Promise<void>
}

export default function AwardAchievementModal({ isOpen, onClose, award, onSave }: AwardAchievementModalProps) {
  const [isSaving, setIsSaving] = useState(false)
  const [formData, setFormData] = useState({
    title: "",
    issuer: "",
    year: "",
    description: "",
    userProfileId: "",
  })

  useEffect(() => {
    if (award) {
      setFormData({
        title: award.title || "",
        issuer: award.issuer || "",
        year: award.year?.toString() || "",
        description: award.description || "",
        userProfileId: award.userProfileId || "",
      })
    } else {
      setFormData({
        title: "",
        issuer: "",
        year: "",
        description: "",
        userProfileId: "",
      })
    }
  }, [award, isOpen])

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }))
  }

  const handleSave = async () => {
    setIsSaving(true)
    try {
      console.log("Award achievement form data:", formData)
      await onSave({
        title: formData.title,
        issuer: formData.issuer,
        year: formData.year ? Number.parseInt(formData.year) : undefined,
        description: formData.description,
        userProfileId: formData.userProfileId,
      })
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{award ? "Edit Award" : "Add Award"}</DialogTitle>
          <DialogDescription>
            {award ? "Update your award details" : "Add a new award or achievement"}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div>
            <Label htmlFor="title">Award Title</Label>
            <Input
              id="title"
              value={formData.title}
              onChange={(e) => handleInputChange("title", e.target.value)}
              placeholder="e.g., Best Developer Award"
            />
          </div>

          <div>
            <Label htmlFor="issuer">Issuer</Label>
            <Input
              id="issuer"
              value={formData.issuer}
              onChange={(e) => handleInputChange("issuer", e.target.value)}
              placeholder="e.g., Tech Company Inc."
            />
          </div>

          <div>
            <Label htmlFor="year">Year</Label>
            <Input
              id="year"
              type="number"
              value={formData.year}
              onChange={(e) => handleInputChange("year", e.target.value)}
              placeholder="e.g., 2023"
            />
          </div>

          <div>
            <Label htmlFor="description">Description</Label>
            <Textarea
              id="description"
              value={formData.description}
              onChange={(e) => handleInputChange("description", e.target.value)}
              placeholder="Describe the award or achievement"
              rows={4}
            />
          </div>
        </div>

        <div className="flex gap-2 justify-end mt-6">
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button onClick={handleSave} disabled={isSaving}>
            {isSaving ? (
              <>
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                Saving...
              </>
            ) : (
              "Save"
            )}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
}
