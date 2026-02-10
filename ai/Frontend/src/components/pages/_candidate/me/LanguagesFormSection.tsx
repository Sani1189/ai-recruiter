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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { UserProfileSchema } from "@/schemas/user-profile";

export default function LanguagesFormSection({
  form,
}: {
  form: UseFormReturn<UserProfileSchema, any, UserProfileSchema>;
}) {
  const { fields, append, remove } = useFieldArray<
    FieldValues & UserProfileSchema
  >({
    control: form.control,
    name: "speakingLanguages",
  });

  const addSpeakingLanguage = () => {
    append({
      language: "",
      proficiency: "",
    });
  };
  const removeSpeakingLanguage = (index: number) => {
    remove(index);
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Skills</CardTitle>
        <CardDescription>Update your skills and expertise</CardDescription>
      </CardHeader>

      <CardContent className="space-y-6">
        {fields.map((edu, index) => (
          <Card key={index}>
            <CardContent className="relative flex items-start justify-between pt-4">
              <div className="flex-1 space-y-4">
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                  <FormField
                    control={form.control}
                    name={`speakingLanguages.${index}.language`}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Language</FormLabel>

                        <Select
                          onValueChange={field.onChange}
                          defaultValue={field.value}
                        >
                          <FormControl className="w-full">
                            <Input
                              placeholder="Enter a speaking language"
                              {...field}
                            />
                          </FormControl>
                        </Select>

                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name={`speakingLanguages.${index}.proficiency`}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Proficiency</FormLabel>

                        <Select
                          onValueChange={field.onChange}
                          defaultValue={field.value}
                        >
                          <FormControl className="w-full">
                            <SelectTrigger className="capitalize">
                              <SelectValue placeholder="Select a proficiency level" />
                            </SelectTrigger>
                          </FormControl>

                          <SelectContent>
                            {UserProfileSchema.shape.speakingLanguages
                              .unwrap()
                              .shape.proficiency.options.map(
                                (proficiency, index) => (
                                  <SelectItem
                                    key={index}
                                    value={proficiency}
                                    className="capitalize"
                                  >
                                    {proficiency}
                                  </SelectItem>
                                ),
                              )}
                          </SelectContent>
                        </Select>

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
                  onClick={() => removeSpeakingLanguage(index)}
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
          onClick={addSpeakingLanguage}
          type="button"
        >
          <Plus className="mr-2 h-4 w-4" />
          Add Speaking Language
        </Button>
      </CardContent>
    </Card>
  );
}
