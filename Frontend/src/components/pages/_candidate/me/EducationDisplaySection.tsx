"use client"

import { Edit2, Plus, Trash2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import type { Education } from "@/types/type"

export default function EducationDisplaySection({
  education,
  onAdd,
  onEdit,
  onDelete,
}: {
  education: Education[] | null
  onAdd: () => void
  onEdit: (edu: Education) => void
  onDelete: (id: string) => void
}) {
  if (!education || education.length === 0) {
    return (
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <CardTitle>Education</CardTitle>
            <CardDescription>Your educational background</CardDescription>
          </div>
          <Button onClick={onAdd} size="sm">
            <Plus className="mr-2 h-4 w-4" />
            Add Education
          </Button>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground">No education records added yet</p>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <CardTitle>Education</CardTitle>
          <CardDescription>Your educational background</CardDescription>
        </div>
        <Button onClick={onAdd} size="sm">
          <Plus className="mr-2 h-4 w-4" />
          Add Education
        </Button>
      </CardHeader>

      <CardContent className="space-y-4">
        {education.map((edu) => (
          <Card key={edu.id} className="border-l-4 border-l-primary">
            <CardContent className="pt-4">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <h4 className="font-semibold">{edu.degree || "Not provided"}</h4>
                  <p className="text-sm text-muted-foreground">{edu.institution || "Not provided"}</p>
                  <p className="text-sm text-muted-foreground">Field of Study: {edu.fieldOfStudy || "Not provided"}</p>
                  <p className="text-sm text-muted-foreground">
                    Graduation Year: {edu.graduationYear || "Not provided"}
                  </p>
                </div>
                <div className="flex gap-2">
                  <Button variant="ghost" size="sm" onClick={() => onEdit(edu)}>
                    <Edit2 className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => edu.id && onDelete(edu.id)}
                    disabled={!edu.id}
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
