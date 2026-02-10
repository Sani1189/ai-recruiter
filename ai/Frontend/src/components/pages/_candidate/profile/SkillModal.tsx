"use client"

import { useState, useEffect } from "react"
import { Loader2 } from "lucide-react"

import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"

import type { SkillDto } from "@/lib/api/services/profile.service"

interface SkillModalProps {
  isOpen: boolean
  onClose: () => void
  skill: SkillDto | null
  onSave: (data: Omit<SkillDto, "id" | "createdAt" | "updatedAt">) => Promise<void>
}

export default function SkillModal({ isOpen, onClose, skill, onSave }: SkillModalProps) {
  const [isSaving, setIsSaving] = useState(false)
  const [formData, setFormData] = useState({
    skillName: "",
    category: "",
    proficiency: "",
    yearsExperience: "",
    unit: "",
    userProfileId: "",
  })

  useEffect(() => {
    if (skill) {
      setFormData({
        skillName: skill.skillName || "",
        category: skill.category || "",
        proficiency: skill.proficiency || "",
        yearsExperience: skill.yearsExperience?.toString() || "",
        unit: skill.unit || "",
        userProfileId: skill.userProfileId || "",
      })
    } else {
      setFormData({
        skillName: "",
        category: "",
        proficiency: "",
        yearsExperience: "",
        unit: "",
        userProfileId: "",
      })
    }
  }, [skill, isOpen])

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }))
  }

  const handleSave = async () => {
    setIsSaving(true)
    try {
      console.log("Skill form data:", formData)
      await onSave({
        ...formData,
        yearsExperience: formData.yearsExperience ? Number.parseInt(formData.yearsExperience) : undefined,
      })
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{skill ? "Edit Skill" : "Add Skill"}</DialogTitle>
          <DialogDescription>{skill ? "Update your skill details" : "Add a new skill"}</DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div>
            <Label htmlFor="skillName">Skill Name</Label>
            <Input
              id="skillName"
              value={formData.skillName}
              onChange={(e) => handleInputChange("skillName", e.target.value)}
              placeholder="e.g., React, Python, etc."
            />
          </div>

          <div>
            <Label htmlFor="category">Category</Label>
            <Input
              id="category"
              value={formData.category}
              onChange={(e) => handleInputChange("category", e.target.value)}
              placeholder="e.g., Programming, Design, etc."
            />
          </div>

          <div>
            <Label htmlFor="proficiency">Proficiency Level</Label>
            <Input
              id="proficiency"
              value={formData.proficiency}
              onChange={(e) => handleInputChange("proficiency", e.target.value)}
              placeholder="e.g., Beginner, Intermediate, Advanced"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label htmlFor="yearsExperience">Years of Experience</Label>
              <Input
                id="yearsExperience"
                type="number"
                value={formData.yearsExperience}
                onChange={(e) => handleInputChange("yearsExperience", e.target.value)}
                placeholder="e.g., 5"
              />
            </div>
            <div>
              <Label htmlFor="unit">Unit</Label>
              <Input
                id="unit"
                value={formData.unit}
                onChange={(e) => handleInputChange("unit", e.target.value)}
                placeholder="e.g., years, months"
              />
            </div>
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
