"use client"

import { useState, useEffect } from "react"
import { Loader2 } from "lucide-react"

import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"

import type { EducationDto } from "@/lib/api/services/profile.service"

interface EducationModalProps {
  isOpen: boolean
  onClose: () => void
  education: EducationDto | null
  onSave: (data: Omit<EducationDto, "id" | "createdAt" | "updatedAt">) => Promise<void>
}

export default function EducationModal({ isOpen, onClose, education, onSave }: EducationModalProps) {
  const [isSaving, setIsSaving] = useState(false)
  const [formData, setFormData] = useState({
    degree: "",
    institution: "",
    fieldOfStudy: "",
    location: "",
    startDate: "",
    endDate: "",
    userProfileId: "",
  })

  useEffect(() => {
    if (education) {
      setFormData({
        degree: education.degree || "",
        institution: education.institution || "",
        fieldOfStudy: education.fieldOfStudy || "",
        location: education.location || "",
        startDate: education.startDate ? education.startDate.split("T")[0] : "",
        endDate: education.endDate ? education.endDate.split("T")[0] : "",
        userProfileId: education.userProfileId || "",
      })
    } else {
      setFormData({
        degree: "",
        institution: "",
        fieldOfStudy: "",
        location: "",
        startDate: "",
        endDate: "",
        userProfileId: "",
      })
    }
  }, [education, isOpen])

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }))
  }

  const handleSave = async () => {
    setIsSaving(true)
    try {
      console.log("Education form data:", formData)
      await onSave(formData)
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{education ? "Edit Education" : "Add Education"}</DialogTitle>
          <DialogDescription>
            {education ? "Update your education details" : "Add a new education record"}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div>
            <Label htmlFor="degree">Degree</Label>
            <Input
              id="degree"
              value={formData.degree}
              onChange={(e) => handleInputChange("degree", e.target.value)}
              placeholder="e.g., Bachelor of Science"
            />
          </div>

          <div>
            <Label htmlFor="institution">Institution</Label>
            <Input
              id="institution"
              value={formData.institution}
              onChange={(e) => handleInputChange("institution", e.target.value)}
              placeholder="e.g., University Name"
            />
          </div>

          <div>
            <Label htmlFor="fieldOfStudy">Field of Study</Label>
            <Input
              id="fieldOfStudy"
              value={formData.fieldOfStudy}
              onChange={(e) => handleInputChange("fieldOfStudy", e.target.value)}
              placeholder="e.g., Computer Science"
            />
          </div>

          <div>
            <Label htmlFor="location">Location</Label>
            <Input
              id="location"
              value={formData.location}
              onChange={(e) => handleInputChange("location", e.target.value)}
              placeholder="e.g., City, Country"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label htmlFor="startDate">Start Date</Label>
              <Input
                id="startDate"
                type="date"
                value={formData.startDate}
                onChange={(e) => handleInputChange("startDate", e.target.value)}
              />
            </div>
            <div>
              <Label htmlFor="endDate">End Date</Label>
              <Input
                id="endDate"
                type="date"
                value={formData.endDate}
                onChange={(e) => handleInputChange("endDate", e.target.value)}
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
