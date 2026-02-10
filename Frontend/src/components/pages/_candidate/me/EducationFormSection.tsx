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

import { UserProfileSchema } from "@/schemas/user-profile";

export default function EducationFormSection({
  form,
}: {
  form: UseFormReturn<UserProfileSchema, any, UserProfileSchema>;
}) {
  const { fields, append, remove } = useFieldArray<
    FieldValues & UserProfileSchema
  >({
    control: form.control,
    name: "education",
  });

  const addEducation = () => {
    append({
      degree: "",
      fieldOfStudy: "",
      institution: "",
      graduationYear: "",
    });
  };
  const removeEducation = (index: number) => {
    remove(index);
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Education</CardTitle>
        <CardDescription>Manage your educational background</CardDescription>
      </CardHeader>

      <CardContent className="space-y-4">
        {fields.map((edu, index) => (
          <Card key={index}>
            <CardContent className="relative flex items-start justify-between pt-4">
              <div className="flex-1 space-y-4">
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                  <FormField
                    control={form.control}
                    name={`education.${index}.degree`}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Degree</FormLabel>

                        <FormControl>
                          <Input {...field} placeholder="Bachelor of Science" />
                        </FormControl>

                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name={`education.${index}.fieldOfStudy`}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Field of Study</FormLabel>

                        <FormControl>
                          <Input {...field} placeholder="Computer Science" />
                        </FormControl>

                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                  <FormField
                    control={form.control}
                    name={`education.${index}.institution`}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Institution</FormLabel>

                        <FormControl>
                          <Input {...field} placeholder="University Name" />
                        </FormControl>

                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name={`education.${index}.graduationYear`}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Graduation Year</FormLabel>

                        <FormControl>
                          <Input {...field} placeholder="2023" />
                        </FormControl>

                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>
              </div>

              {fields.length > 1 && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => removeEducation(index)}
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
          onClick={addEducation}
          type="button"
        >
          <Plus className="mr-2 h-4 w-4" />
          Add Education
        </Button>
      </CardContent>
    </Card>
  );
}
