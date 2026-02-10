"use client";

import {
  FormControl,
  FormDescription,
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
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import { Button } from "@/components/ui/button";
import { MultiSelect } from "@/components/ui/multi-select";
import { Plus, X } from "lucide-react";
import { useFieldArray } from "react-hook-form";
import { useEffect } from "react";

import { useCountries } from "@/hooks/useCountries";
import type { JobPostDetailsStepProps } from "./type";

export default function JobPostDetailsStep({ form }: JobPostDetailsStepProps) {
  const { countries, loading: countriesLoading } = useCountries();
  const { fields, append, remove } = useFieldArray<any>({
    control: form.control as any,
    name: "minimumRequirements",
  }) as any;

  // Ensure there's always at least one minimum requirement field
  useEffect(() => {
    if (fields.length === 0) {
      append("");
    }
  }, [fields.length, append]);

  const status = form.watch("status");
  const countryOptions = countries.map((c) => ({
    label: c.name,
    value: c.countryCode,
  }));
  const ORIGIN_NONE_VALUE = "__none__";

  return (
    <div className="space-y-6">
      {/* Status and Origin Country */}
      <div className="grid gap-6 md:grid-cols-2">
        <FormField
          control={form.control}
          name="status"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Status *</FormLabel>
              <Select
                onValueChange={field.onChange}
                value={field.value ?? "Draft"}
              >
                <FormControl className="w-full">
                  <SelectTrigger>
                    <SelectValue placeholder="Select status" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value="Draft">Draft</SelectItem>
                  <SelectItem value="Published">Published</SelectItem>
                  <SelectItem value="Archived">Archived</SelectItem>
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="originCountryCode"
          render={({ field }) => {
            const hasValue =
              field.value != null && String(field.value).trim() !== "";
            const valueInOptions = countryOptions.some(
              (o) => o.value === field.value,
            );
            const displayValue: string =
              hasValue && (countryOptions.length === 0 || valueInOptions)
                ? (field.value ?? ORIGIN_NONE_VALUE)
                : ORIGIN_NONE_VALUE;
            return (
              <FormItem>
                <FormLabel>Origin Country</FormLabel>
                <Select
                  onValueChange={(v) =>
                    field.onChange(v === ORIGIN_NONE_VALUE ? undefined : v)
                  }
                  value={displayValue}
                  disabled={countriesLoading}
                >
                  <FormControl className="w-full">
                    <SelectTrigger>
                      <SelectValue placeholder="Select origin country" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value={ORIGIN_NONE_VALUE}>None</SelectItem>
                    {countryOptions.map((opt) => (
                      <SelectItem key={opt.value} value={opt.value}>
                        {opt.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            );
          }}
        />
      </div>

      {/* Country exposure (only when Published) */}
      {status === "Published" && (
        <FormField
          control={form.control}
          name="countryExposureCountryCodes"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Expose to countries</FormLabel>
              <FormControl>
                <MultiSelect
                  options={countryOptions}
                  onValueChange={field.onChange}
                  defaultValue={field.value ?? []}
                />
              </FormControl>
              <FormDescription>
                Select the countries where this job post will be visible.
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />
      )}

      {/* Job Title and Type */}
      <div className="grid gap-6 md:grid-cols-2">
        <FormField
          control={form.control}
          name="jobTitle"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Job Title *</FormLabel>
              <FormControl>
                <Input placeholder="e.g., Senior React Developer" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="jobType"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Job Type *</FormLabel>
              <Select onValueChange={field.onChange} defaultValue={field.value}>
                <FormControl className="w-full">
                  <SelectTrigger>
                    <SelectValue placeholder="Select job type" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value="FullTime">Full Time</SelectItem>
                  <SelectItem value="PartTime">Part Time</SelectItem>
                  <SelectItem value="Contract">Contract</SelectItem>
                  <SelectItem value="Internship">Internship</SelectItem>
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />
      </div>

      {/* Experience Level and Max Candidates */}
      <div className="grid gap-6 md:grid-cols-2">
        <FormField
          control={form.control}
          name="experienceLevel"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Experience Level *</FormLabel>
              <Select onValueChange={field.onChange} defaultValue={field.value}>
                <FormControl className="w-full">
                  <SelectTrigger>
                    <SelectValue placeholder="Select experience level" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value="Entry">Entry</SelectItem>
                  <SelectItem value="Mid">Mid</SelectItem>
                  <SelectItem value="Senior">Senior</SelectItem>
                  <SelectItem value="Lead">Lead</SelectItem>
                  <SelectItem value="Executive">Executive</SelectItem>
                </SelectContent>
              </Select>
              <FormDescription>
                Select the experience level required for this position.
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="maxAmountOfCandidatesRestriction"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Max Number of Candidates *</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  placeholder="e.g., 100"
                  {...field}
                  onChange={(e) =>
                    field.onChange(parseInt(e.target.value) || 0)
                  }
                />
              </FormControl>
              <FormDescription>
                Maximum number of candidates allowed for this job post.
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />
      </div>

      {/* Job Description */}
      <FormField
        control={form.control}
        name="jobDescription"
        render={({ field }) => (
          <FormItem>
            <FormLabel>Job Description *</FormLabel>
            <FormControl>
              <Textarea
                placeholder="Enter the job description here..."
                className="min-h-[120px]"
                {...field}
              />
            </FormControl>
            <FormDescription>
              Provide a detailed description of the role, responsibilities, and
              requirements.
            </FormDescription>
            <FormMessage />
          </FormItem>
        )}
      />

      {/* Minimum Requirements */}
      <div className="space-y-4">
        <FormLabel>Minimum Requirements *</FormLabel>

        <div className="space-y-3">
          {fields.map((field: { id: string }, index: number) => (
            <FormField
              key={field.id}
              control={form.control}
              name={`minimumRequirements.${index}`}
              render={({ field }) => (
                <FormItem>
                  <div className="flex gap-2">
                    <FormControl>
                      <Input
                        placeholder={`Requirement ${index + 1}`}
                        {...field}
                      />
                    </FormControl>
                    {fields.length > 1 && (
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        onClick={() => remove(index)}
                      >
                        <X className="h-4 w-4" />
                      </Button>
                    )}
                  </div>
                  <FormMessage />
                </FormItem>
              )}
            />
          ))}
        </div>

        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => append("")}
          className="w-full"
        >
          <Plus className="mr-2 h-4 w-4" />
          Add Requirement
        </Button>
      </div>

      {/* Police Report */}
      <FormField
        control={form.control}
        name="policeReportRequired"
        render={({ field }) => (
          <FormItem>
            <FormLabel className="hover:bg-accent/50 has-[[aria-checked=true]]:border-primary has-[[aria-checked=true]]:bg-primary/5 flex cursor-pointer items-start gap-3 rounded-lg border p-4">
              <FormControl>
                <Checkbox
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              </FormControl>
              <div className="grid gap-1.5 font-normal">
                <p className="text-sm leading-none font-medium">
                  Police Report Required
                </p>
                <p className="text-muted-foreground text-xs">
                  Candidates must provide a police report
                </p>
              </div>
            </FormLabel>
            <FormMessage />
          </FormItem>
        )}
      />
    </div>
  );
}
