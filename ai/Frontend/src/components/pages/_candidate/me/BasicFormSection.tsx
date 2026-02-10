"use client";

import {
  Briefcase,
  Building,
  Clock9,
  FileText,
  GraduationCap,
  LucideProps,
  Shuffle,
  Wifi,
} from "lucide-react";
import { UseFormReturn } from "react-hook-form";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

import { UserProfileSchema } from "@/schemas/user-profile";
import type { UserProfile } from "@/types/v2/type";
import { ForwardRefExoticComponent, RefAttributes, useState } from "react";

const JOB_TYPE_PREFERENCES: {
  label: string;
  value: UserProfileSchema["jobTypePreferences"][number];
  icon: ForwardRefExoticComponent<
    Omit<LucideProps, "ref"> & RefAttributes<SVGSVGElement>
  >;
}[] = [
  {
    label: "Full Time",
    value: "full-time",
    icon: Briefcase,
  },
  {
    label: "Part Time",
    value: "part-time",
    icon: Clock9,
  },
  {
    label: "Internship",
    value: "internship",
    icon: GraduationCap,
  },
  {
    label: "Contract",
    value: "contract",
    icon: FileText,
  },
];
const REMOTE_PREFERENCES: {
  label: string;
  value: UserProfileSchema["remotePreferences"][number];
  icon: ForwardRefExoticComponent<
    Omit<LucideProps, "ref"> & RefAttributes<SVGSVGElement>
  >;
}[] = [
  {
    label: "Remote",
    value: "remote",
    icon: Wifi,
  },
  {
    label: "Hybrid",
    value: "hybrid",
    icon: Shuffle,
  },
  {
    label: "Onsite",
    value: "on-site",
    icon: Building,
  },
];

export default function BasicFormSection({
  form,
  profile,
}: {
  form: UseFormReturn<UserProfileSchema, any, UserProfileSchema>;
  profile: UserProfile;
}) {
  const [pfp, setPfp] = useState<string>(profile.profilePictureUrl ?? "");

  const handlePhotoClick = () => {
    // Trigger file input click to change profile photo
    const fileInput = document.createElement("input");
    fileInput.type = "file";
    fileInput.accept = "image/*";
    fileInput.onchange = (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (file) {
        const reader = new FileReader();
        reader.onloadend = () => {
          setPfp(reader.result as string);
          // Here you would typically also update the profile picture URL in your backend
          // and possibly call a function to update the form state with the new URL.
          form.setValue("profilePictureUrl", reader.result as string);
        };
        reader.readAsDataURL(file);
      }
    };
    fileInput.click();
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Basic Information</CardTitle>
        <CardDescription>
          Update your personal details and preferences
        </CardDescription>
      </CardHeader>

      <CardContent className="space-y-6">
        <div className="flex items-center gap-6">
          <div
            className="group relative cursor-pointer"
            onClick={handlePhotoClick}
          >
            <Avatar className="h-32 w-32 transition-all duration-200 group-hover:opacity-75">
              <AvatarImage
                src={pfp}
                alt={profile.name ?? "Profile picture"}
                className="object-cover"
              />

              <AvatarFallback className="text-2xl">
                {profile.name?.slice(0, 2).toUpperCase() || "NA"}
              </AvatarFallback>
            </Avatar>

            <div className="absolute inset-0 flex items-center justify-center rounded-full bg-black/50 opacity-0 transition-opacity duration-200 group-hover:opacity-100">
              <span className="text-sm font-medium text-white">
                Change Photo
              </span>
            </div>
          </div>

          <div>
            <h3 className="text-lg font-semibold">{profile.name}</h3>
            <p className="text-muted-foreground">{profile.email}</p>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Full Name</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="email"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Email</FormLabel>
                <FormControl>
                  <Input type="email" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
          <FormField
            control={form.control}
            name="phoneNumber"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Phone Number</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="age"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Age</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min="18"
                    max="100"
                    {...field}
                    onChange={(e) => field.onChange(parseInt(e.target.value))}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="nationality"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Nationality</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="bio"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Bio</FormLabel>
              <FormControl>
                <Textarea className="min-h-[120px]" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="space-y-4">
          <FormLabel>Job Type Preferences</FormLabel>

          <div className="flex flex-wrap gap-4">
            {JOB_TYPE_PREFERENCES.map(({ value, label, icon: Icon }) => (
              <FormField
                key={value}
                control={form.control}
                name="jobTypePreferences"
                render={({ field }) => (
                  <FormItem className="flex grow items-center space-x-2">
                    <FormLabel className="hover:bg-accent/50 has-[[aria-checked=true]]:border-primary has-[[aria-checked=true]]:bg-primary/5 dark:has-[[aria-checked=true]]:border-primary dark:has-[[aria-checked=true]]:bg-primary flex w-full cursor-pointer items-start gap-3 rounded-lg border p-3">
                      {/* Hidden from ui */}
                      <FormControl>
                        <Checkbox
                          className="hidden"
                          checked={field.value?.includes(value)}
                          onCheckedChange={(checked) => {
                            if (checked) {
                              field.onChange([...field.value, value]);
                            } else {
                              field.onChange(
                                field.value?.filter((item: typeof value) => item !== value),
                              );
                            }
                          }}
                        />
                      </FormControl>

                      <Icon className="h-4 w-4" />

                      <p className="text-sm leading-none font-medium capitalize">
                        {label}
                      </p>
                    </FormLabel>
                  </FormItem>
                )}
              />
            ))}
          </div>
        </div>

        <div className="space-y-4">
          <FormLabel>Remote Work Preferences</FormLabel>

          <div className="flex flex-wrap gap-4">
            {REMOTE_PREFERENCES.map(({ value, label, icon: Icon }) => (
              <FormField
                key={value}
                control={form.control}
                name="remotePreferences"
                render={({ field }) => (
                  <FormItem className="flex grow items-center space-x-2">
                    <FormLabel className="hover:bg-accent/50 has-[[aria-checked=true]]:border-primary has-[[aria-checked=true]]:bg-primary/5 dark:has-[[aria-checked=true]]:border-primary dark:has-[[aria-checked=true]]:bg-primary flex w-full cursor-pointer items-start gap-3 rounded-lg border p-3">
                      {/* Hidden from ui */}
                      <FormControl>
                        <Checkbox
                          className="hidden"
                          checked={field.value?.includes(value)}
                          onCheckedChange={(checked) => {
                            if (checked) {
                              field.onChange([...field.value, value]);
                            } else {
                              field.onChange(
                                field.value?.filter((item: typeof value) => item !== value),
                              );
                            }
                          }}
                        />
                      </FormControl>

                      <Icon className="h-4 w-4" />

                      <p className="text-sm leading-none font-medium capitalize">
                        {label}
                      </p>
                    </FormLabel>
                  </FormItem>
                )}
              />
            ))}
          </div>
        </div>

        <FormField
          control={form.control}
          name="openToRelocation"
          render={({ field }) => (
            <FormItem className="flex items-center space-x-2">
              <FormLabel className="hover:bg-accent/50 has-[[aria-checked=true]]:border-primary has-[[aria-checked=true]]:bg-primary/5 dark:has-[[aria-checked=true]]:border-primary dark:has-[[aria-checked=true]]:bg-primary flex w-full cursor-pointer items-start gap-3 rounded-lg border p-3">
                <FormControl>
                  <Checkbox
                    className="data-[state=checked]:border-primary data-[state=checked]:bg-primary dark:data-[state=checked]:border-primary dark:data-[state=checked]:bg-primary data-[state=checked]:text-white"
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                </FormControl>

                <div className="grid gap-1.5 font-normal">
                  <p className="text-sm leading-none font-medium">
                    Open to Relocation
                  </p>

                  <p className="text-muted-foreground text-sm">
                    Indicate if you are willing to relocate for a job
                  </p>
                </div>
              </FormLabel>
            </FormItem>
          )}
        />
      </CardContent>
    </Card>
  );
}
