"use client"

import { useState, useEffect } from "react"
import { Loader2 } from "lucide-react"

import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"

import type { CertificationLicenseDto } from "@/lib/api/services/profile.service"

interface CertificationModalProps {
  isOpen: boolean
  onClose: () => void
  certification: CertificationLicenseDto | null
  onSave: (data: Omit<CertificationLicenseDto, "id" | "createdAt" | "updatedAt">) => Promise<void>
}

export default function CertificationModal({ isOpen, onClose, certification, onSave }: CertificationModalProps) {
  const [isSaving, setIsSaving] = useState(false)
  const [formData, setFormData] = useState({
    name: "",
    issuer: "",
    dateIssued: "",
    validUntil: "",
    userProfileId: "",
  })

  useEffect(() => {
    if (certification) {
      setFormData({
        name: certification.name || "",
        issuer: certification.issuer || "",
        dateIssued: certification.dateIssued ? certification.dateIssued.split("T")[0] : "",
        validUntil: certification.validUntil ? certification.validUntil.split("T")[0] : "",
        userProfileId: certification.userProfileId || "",
      })
    } else {
      setFormData({
        name: "",
        issuer: "",
        dateIssued: "",
        validUntil: "",
        userProfileId: "",
      })
    }
  }, [certification, isOpen])

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }))
  }

  const handleSave = async () => {
    setIsSaving(true)
    try {
      console.log("Certification form data:", formData)
      await onSave(formData)
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{certification ? "Edit Certification" : "Add Certification"}</DialogTitle>
          <DialogDescription>
            {certification ? "Update your certification details" : "Add a new certification or license"}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div>
            <Label htmlFor="name">Certification Name</Label>
            <Input
              id="name"
              value={formData.name}
              onChange={(e) => handleInputChange("name", e.target.value)}
              placeholder="e.g., AWS Certified Solutions Architect"
            />
          </div>

          <div>
            <Label htmlFor="issuer">Issuer</Label>
            <Input
              id="issuer"
              value={formData.issuer}
              onChange={(e) => handleInputChange("issuer", e.target.value)}
              placeholder="e.g., Amazon Web Services"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label htmlFor="dateIssued">Date Issued</Label>
              <Input
                id="dateIssued"
                type="date"
                value={formData.dateIssued}
                onChange={(e) => handleInputChange("dateIssued", e.target.value)}
              />
            </div>
            <div>
              <Label htmlFor="validUntil">Valid Until</Label>
              <Input
                id="validUntil"
                type="date"
                value={formData.validUntil}
                onChange={(e) => handleInputChange("validUntil", e.target.value)}
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
