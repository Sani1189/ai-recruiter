"use client"

import { useState, useEffect } from "react"
import { Loader2 } from "lucide-react"

import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"

import type { SummaryDto } from "@/lib/api/services/profile.service"

interface SummaryModalProps {
  isOpen: boolean
  onClose: () => void
  summary: SummaryDto | null
  onSave: (data: Omit<SummaryDto, "id" | "createdAt" | "updatedAt">) => Promise<void>
}

export default function SummaryModal({ isOpen, onClose, summary, onSave }: SummaryModalProps) {
  const [isSaving, setIsSaving] = useState(false)
  const [formData, setFormData] = useState({
    type: "",
    text: "",
    userProfileId: "",
  })

  useEffect(() => {
    if (summary) {
      setFormData({
        type: summary.type || "",
        text: summary.text || "",
        userProfileId: summary.userProfileId || "",
      })
    } else {
      setFormData({
        type: "",
        text: "",
        userProfileId: "",
      })
    }
  }, [summary, isOpen])

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }))
  }

  const handleSave = async () => {
    setIsSaving(true)
    try {
      console.log("Summary form data:", formData)
      await onSave(formData)
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{summary ? "Edit Summary" : "Add Summary"}</DialogTitle>
          <DialogDescription>
            {summary ? "Update your professional summary" : "Add a professional summary"}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div>
            <Label htmlFor="type">Summary Type</Label>
            <Input
              id="type"
              value={formData.type}
              onChange={(e) => handleInputChange("type", e.target.value)}
              placeholder="e.g., Professional Summary"
            />
          </div>

          <div>
            <Label htmlFor="text">Summary Text</Label>
            <Textarea
              id="text"
              value={formData.text}
              onChange={(e) => handleInputChange("text", e.target.value)}
              placeholder="Write your professional summary here..."
              rows={6}
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
