"use client"

import { useState } from "react"
import { Loader2 } from "lucide-react"

import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Checkbox } from "@/components/ui/checkbox"
import { Label } from "@/components/ui/label"

import type { UserProfileDto } from "@/lib/api/services/profile.service"

interface EditProfileModalProps {
  isOpen: boolean
  onClose: () => void
  profile: UserProfileDto
  onSave: (data: Partial<UserProfileDto>) => Promise<void>
}

export default function EditProfileModal({ isOpen, onClose, profile, onSave }: EditProfileModalProps) {
  const [isSaving, setIsSaving] = useState(false)
  const [formData, setFormData] = useState<Partial<UserProfileDto>>({
    name: profile.name || "",
    email: profile.email || "",
    phoneNumber: profile.phoneNumber || "",
    age: profile.age || 0,
    nationality: profile.nationality || "",
    bio: profile.bio || "",
    openToRelocation: profile.openToRelocation || false,
    jobTypePreferences: profile.jobTypePreferences || [],
    remotePreferences: profile.remotePreferences || [],
  })

  const handleInputChange = (field: string, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }))
  }

  const handleJobTypeToggle = (jobType: string) => {
    setFormData((prev) => ({
      ...prev,
      jobTypePreferences: prev.jobTypePreferences?.includes(jobType as any)
        ? prev.jobTypePreferences.filter((j) => j !== jobType)
        : [...(prev.jobTypePreferences || []), jobType as any],
    }))
  }

  const handleRemotePreferenceToggle = (pref: string) => {
    setFormData((prev) => ({
      ...prev,
      remotePreferences: prev.remotePreferences?.includes(pref as any)
        ? prev.remotePreferences.filter((r) => r !== pref)
        : [...(prev.remotePreferences || []), pref as any],
    }))
  }

  const handleSave = async () => {
    setIsSaving(true)
    try {
      await onSave(formData)
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Edit Profile</DialogTitle>
          <DialogDescription>Update your personal information</DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          {/* Name and Email */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label htmlFor="name">Full Name</Label>
              <Input
                id="name"
                value={formData.name || ""}
                onChange={(e) => handleInputChange("name", e.target.value)}
              />
            </div>
            <div>
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                value={formData.email || ""}
                onChange={(e) => handleInputChange("email", e.target.value)}
              />
            </div>
          </div>

          {/* Phone, Age, Nationality */}
          <div className="grid grid-cols-3 gap-4">
            <div>
              <Label htmlFor="phone">Phone Number</Label>
              <Input
                id="phone"
                value={formData.phoneNumber || ""}
                onChange={(e) => handleInputChange("phoneNumber", e.target.value)}
              />
            </div>
            <div>
              <Label htmlFor="age">Age</Label>
              <Input
                id="age"
                type="number"
                value={formData.age || ""}
                onChange={(e) => handleInputChange("age", Number.parseInt(e.target.value))}
              />
            </div>
            <div>
              <Label htmlFor="nationality">Nationality</Label>
              <Input
                id="nationality"
                value={formData.nationality || ""}
                onChange={(e) => handleInputChange("nationality", e.target.value)}
              />
            </div>
          </div>

          {/* Bio */}
          <div>
            <Label htmlFor="bio">Bio</Label>
            <Textarea
              id="bio"
              value={formData.bio || ""}
              onChange={(e) => handleInputChange("bio", e.target.value)}
              rows={4}
            />
          </div>

          {/* Job Type Preferences */}
          <div>
            <Label className="mb-2 block">Job Type Preferences</Label>
            <div className="space-y-2">
              {["full-time", "part-time", "internship", "contract"].map((jobType) => (
                <div key={jobType} className="flex items-center gap-2">
                  <Checkbox
                    id={jobType}
                    checked={formData.jobTypePreferences?.includes(jobType as any) || false}
                    onCheckedChange={() => handleJobTypeToggle(jobType)}
                  />
                  <Label htmlFor={jobType} className="capitalize cursor-pointer">
                    {jobType}
                  </Label>
                </div>
              ))}
            </div>
          </div>

          {/* Remote Preferences */}
          <div>
            <Label className="mb-2 block">Remote Work Preferences</Label>
            <div className="space-y-2">
              {["remote", "hybrid", "on-site"].map((pref) => (
                <div key={pref} className="flex items-center gap-2">
                  <Checkbox
                    id={pref}
                    checked={formData.remotePreferences?.includes(pref as any) || false}
                    onCheckedChange={() => handleRemotePreferenceToggle(pref)}
                  />
                  <Label htmlFor={pref} className="capitalize cursor-pointer">
                    {pref}
                  </Label>
                </div>
              ))}
            </div>
          </div>

          {/* Open to Relocation */}
          <div className="flex items-center gap-2">
            <Checkbox
              id="relocation"
              checked={formData.openToRelocation || false}
              onCheckedChange={(checked) => handleInputChange("openToRelocation", checked)}
            />
            <Label htmlFor="relocation" className="cursor-pointer">
              Open to Relocation
            </Label>
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
              "Save Changes"
            )}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
}
