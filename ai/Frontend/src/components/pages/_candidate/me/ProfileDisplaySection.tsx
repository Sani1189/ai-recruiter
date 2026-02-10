"use client"

import { Edit2, Plus } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import type { UserProfile } from "@/types/type"

export default function ProfileDisplaySection({
  profile,
  onEdit,
}: {
  profile: UserProfile | null
  onEdit: () => void
}) {
  if (!profile) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Profile Information</CardTitle>
          <CardDescription>Your personal details</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-between">
            <p className="text-muted-foreground">No profile data available</p>
            <Button onClick={onEdit} size="sm">
              <Plus className="mr-2 h-4 w-4" />
              Create Profile
            </Button>
          </div>
        </CardContent>
      </Card>
    )
  }

  const profileName = profile.name || "Not provided"
  const profileEmail = profile.email || "Not provided"

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <CardTitle>Profile Information</CardTitle>
          <CardDescription>Your personal details</CardDescription>
        </div>
        <Button onClick={onEdit} size="sm" variant="outline">
          <Edit2 className="mr-2 h-4 w-4" />
          Edit
        </Button>
      </CardHeader>

      <CardContent className="space-y-6">
        <div className="flex items-center gap-6">
          <Avatar className="h-24 w-24">
            <AvatarImage src={profile.profilePictureUrl || "/placeholder.svg"} alt={profileName} />
            <AvatarFallback>{profileName.slice(0, 2).toUpperCase()}</AvatarFallback>
          </Avatar>
          <div>
            <h3 className="text-lg font-semibold">{profileName}</h3>
            <p className="text-muted-foreground">{profileEmail}</p>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <div>
            <p className="text-sm font-medium text-muted-foreground">Phone Number</p>
            <p className="text-sm">{profile.phoneNumber || "Not provided"}</p>
          </div>
          <div>
            <p className="text-sm font-medium text-muted-foreground">Age</p>
            <p className="text-sm">{profile.age ? `${profile.age} years` : "Not provided"}</p>
          </div>
          <div>
            <p className="text-sm font-medium text-muted-foreground">Nationality</p>
            <p className="text-sm">{profile.nationality || "Not provided"}</p>
          </div>
          <div>
            <p className="text-sm font-medium text-muted-foreground">Open to Relocation</p>
            <p className="text-sm">{profile.openToRelocation ? "Yes" : "No"}</p>
          </div>
        </div>

        {profile.bio && (
          <div>
            <p className="text-sm font-medium text-muted-foreground">Bio</p>
            <p className="text-sm">{profile.bio}</p>
          </div>
        )}

        {profile.jobTypePreferences && profile.jobTypePreferences.length > 0 && (
          <div>
            <p className="text-sm font-medium text-muted-foreground">Job Type Preferences</p>
            <div className="flex flex-wrap gap-2">
              {profile.jobTypePreferences.map((pref) => (
                <span
                  key={pref}
                  className="inline-block rounded-full bg-primary/10 px-3 py-1 text-xs font-medium capitalize"
                >
                  {pref}
                </span>
              ))}
            </div>
          </div>
        )}

        {profile.remotePreferences && profile.remotePreferences.length > 0 && (
          <div>
            <p className="text-sm font-medium text-muted-foreground">Remote Work Preferences</p>
            <div className="flex flex-wrap gap-2">
              {profile.remotePreferences.map((pref) => (
                <span
                  key={pref}
                  className="inline-block rounded-full bg-primary/10 px-3 py-1 text-xs font-medium capitalize"
                >
                  {pref}
                </span>
              ))}
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
