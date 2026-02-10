"use client"

import { Edit2, Plus, Trash2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import type { Skill } from "@/types/type"

export default function SkillsDisplaySection({
  skills,
  onAdd,
  onEdit,
  onDelete,
}: {
  skills: Skill[] | null
  onAdd: () => void
  onEdit: (skill: Skill) => void
  onDelete: (id: string) => void
}) {
  if (!skills || skills.length === 0) {
    return (
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <CardTitle>Skills</CardTitle>
            <CardDescription>Your professional skills</CardDescription>
          </div>
          <Button onClick={onAdd} size="sm">
            <Plus className="mr-2 h-4 w-4" />
            Add Skill
          </Button>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground">No skills added yet</p>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <CardTitle>Skills</CardTitle>
          <CardDescription>Your professional skills</CardDescription>
        </div>
        <Button onClick={onAdd} size="sm">
          <Plus className="mr-2 h-4 w-4" />
          Add Skill
        </Button>
      </CardHeader>

      <CardContent className="space-y-4">
        {skills.map((skill) => (
          <Card key={skill.id} className="border-l-4 border-l-primary">
            <CardContent className="pt-4">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <h4 className="font-semibold">{skill.skillName || "Not provided"}</h4>
                  <p className="text-sm text-muted-foreground">
                    Proficiency: {skill.proficiencyLevel || "Not provided"}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    Years of Experience: {skill.yearsOfExperience || "Not provided"}
                  </p>
                </div>
                <div className="flex gap-2">
                  <Button variant="ghost" size="sm" onClick={() => onEdit(skill)}>
                    <Edit2 className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => skill.id && onDelete(skill.id)}
                    disabled={!skill.id}
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
