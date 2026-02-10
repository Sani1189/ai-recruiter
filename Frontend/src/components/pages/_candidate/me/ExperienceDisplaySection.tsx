"use client"

import { Edit2, Plus, Trash2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import type { Experience } from "@/types/type"

export default function ExperienceDisplaySection({
  experience,
  onAdd,
  onEdit,
  onDelete,
}: {
  experience: Experience[] | null
  onAdd: () => void
  onEdit: (exp: Experience) => void
  onDelete: (id: string) => void
}) {
  if (!experience || experience.length === 0) {
    return (
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <CardTitle>Work Experience</CardTitle>
            <CardDescription>Your professional history</CardDescription>
          </div>
          <Button onClick={onAdd} size="sm">
            <Plus className="mr-2 h-4 w-4" />
            Add Experience
          </Button>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground">No work experience added yet</p>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <CardTitle>Work Experience</CardTitle>
          <CardDescription>Your professional history</CardDescription>
        </div>
        <Button onClick={onAdd} size="sm">
          <Plus className="mr-2 h-4 w-4" />
          Add Experience
        </Button>
      </CardHeader>

      <CardContent className="space-y-4">
        {experience.map((exp) => (
          <Card key={exp.id} className="border-l-4 border-l-primary">
            <CardContent className="pt-4">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <h4 className="font-semibold">{exp.jobTitle || "Not provided"}</h4>
                  <p className="text-sm text-muted-foreground">{exp.company || "Not provided"}</p>
                  <p className="text-sm text-muted-foreground">
                    {exp.startDate ? new Date(exp.startDate).toLocaleDateString() : "Not provided"} -{" "}
                    {exp.endDate ? new Date(exp.endDate).toLocaleDateString() : "Not provided"}
                  </p>
                  {exp.description && <p className="mt-2 text-sm">{exp.description}</p>}
                </div>
                <div className="flex gap-2">
                  <Button variant="ghost" size="sm" onClick={() => onEdit(exp)}>
                    <Edit2 className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => exp.id && onDelete(exp.id)}
                    disabled={!exp.id}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </CardContent>
    </Card>
  )
}
