"use client"

import { useState, useEffect } from "react"
import { Loader2 } from "lucide-react"

import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"

import type { KeyStrengthDto } from "@/lib/api/services/profile.service"

interface KeyStrengthModalProps {
  isOpen: boolean
  onClose: () => void
  strength: KeyStrengthDto | null
  onSave: (data: Omit<KeyStrengthDto, "id" | "createdAt" | "updatedAt">) => Promise<void>
}

export default function KeyStrengthModal({ isOpen, onClose, strength, onSave }: KeyStrengthModalProps) {
  const [isSaving, setIsSaving] = useState(false)
  const [formData, setFormData] = useState({
    strengthName: "",
    description: "",
    userProfileId: "",
  })

  useEffect(() => {
    if (strength) {
      setFormData({
        strengthName: strength.strengthName || "",
        description: strength.description || "",
        userProfileId: strength.userProfileId || "",
      })
    } else {
      setFormData({
        strengthName: "",
        description: "",
        userProfileId: "",
      })
    }
  }, [strength, isOpen])

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }))
  }

  const handleSave = async () => {
    setIsSaving(true)
    try {
      console.log("Key strength form data:", formData)
      await onSave(formData)
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{strength ? "Edit Strength" : "Add Strength"}</DialogTitle>
          <DialogDescription>{strength ? "Update your strength details" : "Add a new key strength"}</DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div>
            <Label htmlFor="strengthName">Strength Name</Label>
            <Input
              id="strengthName"
              value={formData.strengthName}
              onChange={(e) => handleInputChange("strengthName", e.target.value)}
              placeholder="e.g., Leadership"
            />
          </div>

          <div>
            <Label htmlFor="description">Description</Label>
            <Textarea
              id="description"
              value={formData.description}
              onChange={(e) => handleInputChange("description", e.target.value)}
              placeholder="Describe this strength"
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
