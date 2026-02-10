"use client";

import { Plus, X } from "lucide-react";
import { FieldValues, useFieldArray, UseFormReturn } from "react-hook-form";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { MultiSelect } from "@/components/ui/multi-select";
import { Textarea } from "@/components/ui/textarea";

import { UserProfileSchema } from "@/schemas/user-profile";

const frameworksList: {
  value: UserProfileSchema["projects"][number]["technologies"][number];
  label: string;
}[] = [
  {
    value: "AWS",
    label: "AWS",
  },
  {
    value: "Azure",
    label: "React",
  },
  {
    value: "C",
    label: "C",
  },
  {
    value: "C++",
    label: "C++",
  },
  {
    value: "C#",
    label: "C#",
  },
  {
    value: "CSS",
    label: "CSS",
  },
  {
    value: "Dart",
    label: "Dart",
  },
  {
    value: "Docker",
    label: "Docker",
  },
  {
    value: "Elixir",
    label: "Elixir",
  },
  {
    value: "Firebase",
    label: "Firebase",
  },
  {
    value: "GCP",
    label: "GCP",
  },
  {
    value: "Go",
    label: "Go",
  },
  {
    value: "GraphQL",
    label: "GraphQL",
  },
  {
    value: "HTML",
    label: "HTML",
  },
  {
    value: "Java",
    label: "Java",
  },
  {
    value: "JavaScript",
    label: "JavaScript",
  },
  {
    value: "Kotlin",
    label: "Kotlin",
  },
  {
    value: "MySQL",
    label: "MySQL",
  },
  {
    value: "Kubernetes",
    label: "Kubernetes",
  },
  {
    value: "MongoDB",
    label: "MongoDB",
  },
  {
    value: "NoSQL",
    label: "NoSQL",
  },
  {
    value: "PHP",
    label: "PHP",
  },
  {
    value: "PostgreSQL",
    label: "PostgreSQL",
  },
  {
    value: "Python",
    label: "Python",
  },
  {
    value: "Redis",
    label: "Redis",
  },
  {
    value: "Ruby",
    label: "Ruby",
  },
  {
    value: "Rust",
    label: "Rust",
  },
  {
    value: "SQL",
    label: "SQL",
  },
  {
    value: "SQLite",
    label: "SQLite",
  },
  {
    value: "Scala",
    label: "Scala",
  },
  {
    value: "Swift",
    label: "Swift",
  },
  {
    value: "TypeScript",
    label: "TypeScript",
  },
];

export default function ProjectsFormSection({
  form,
}: {
  form: UseFormReturn<UserProfileSchema, any, UserProfileSchema>;
}) {
  const { fields, append, remove } = useFieldArray<
    FieldValues & UserProfileSchema
  >({
    control: form.control,
    name: "projects",
  });

  const addProject = () => {
    append({
      title: "",
      description: "",
      url: "",
      technologies: [],
    });
  };
  const removeProject = (index: number) => {
    remove(index);
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Projects</CardTitle>
        <CardDescription>Showcase your work and achievements</CardDescription>
      </CardHeader>

      <CardContent className="space-y-4">
        {fields.map((project, index) => (
          <Card key={index}>
            <CardContent className="relative flex items-start justify-between pt-4">
              <div className="flex-1 space-y-4">
                <FormField
                  control={form.control}
                  name={`projects.${index}.title`}
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Project Title</FormLabel>

                      <FormControl>
                        <Input {...field} placeholder="E-commerce Platform" />
                      </FormControl>

                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name={`projects.${index}.description`}
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Description</FormLabel>

                      <FormControl>
                        <Textarea
                          {...field}
                          placeholder="Brief description of the project..."
                        />
                      </FormControl>

                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name={`projects.${index}.url`}
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Project URL</FormLabel>

                      <FormControl>
                        <Input {...field} placeholder="Enter project URL" />
                      </FormControl>

                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name={`projects.${index}.technologies`}
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Technologies</FormLabel>

                      <FormControl>
                        <MultiSelect
                          options={frameworksList}
                          onValueChange={field.onChange}
                          defaultValue={field.value}
                          placeholder="Select options"
                          variant="inverted"
                          maxCount={6}
                        />
                      </FormControl>

                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {fields.length > 1 && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => removeProject(index)}
                  type="button"
                  className="absolute -top-4 right-2"
                >
                  <X className="size-3" />
                </Button>
              )}
            </CardContent>
          </Card>
        ))}

        <Button
          variant="outline"
          className="w-full"
          onClick={addProject}
          type="button"
        >
          <Plus className="mr-2 h-4 w-4" />
          Add Project
        </Button>
      </CardContent>
    </Card>
  );
}
