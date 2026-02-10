"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { Briefcase } from "lucide-react";
import { Fragment, useEffect } from "react";
import { useForm, type DefaultValues } from "react-hook-form";
import z from "zod";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Form } from "@/components/ui/form";
import { Separator } from "@/components/ui/separator";
import { defineStepper } from "@/components/ui/stepper";

import JobPostDetailsStep from "./JobPostDetailsStep";
import JobStepsStep from "./JobStepsStep";
import ConfirmationStep from "./ConfirmationStep";

import JobPostCreationForm, {
  JobPostDetails,
  JobSteps,
  Confirmation,
} from "@/schemas/job-posting";

const { useStepper, steps, utils } = defineStepper(
  { id: "jobPostDetails", label: "Job Post Details", schema: JobPostDetails },
  { id: "jobSteps", label: "Job Steps", schema: JobSteps },
  { id: "confirmation", label: "Confirmation", schema: Confirmation },
);

type JobPostFormProps = {
  variant: "create" | "edit";
  onSubmit: (data: JobPostCreationForm) => void | Promise<void>;
  defaultValues: DefaultValues<JobPostCreationForm>;
};

export default function JobPostForm({
  variant,
  onSubmit,
  defaultValues,
}: JobPostFormProps) {
  const stepper = useStepper();
  const currentIndex = utils.getIndex(stepper.current.id);

  // Use the FULL form schema instead of the current step schema
  // This ensures all form data is preserved across steps
  const form = useForm<JobPostCreationForm>({
    resolver: zodResolver(stepper.current.schema as any),
    defaultValues,
    mode: "onChange",
    shouldUseNativeValidation: false,
  });

  // Reset form when defaultValues change (for edit mode when data loads async)
  // Only reset if we're starting fresh, not during submission
  useEffect(() => {
    if (defaultValues && !form.formState.isDirty) {
      form.reset(defaultValues as any);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [defaultValues]);

  const handleSubmit = async () => {
    if (stepper.isLast) {
      // On final step, get ALL form values and call parent's onSubmit
      // Using getValues() ensures we get the complete form data, not just current step
      const allFormData = form.getValues() as JobPostCreationForm;
      await onSubmit(allFormData);
    } else {
      // Validate current step before proceeding
      const isValid = await form.trigger();
      if (!isValid) {
        return;
      }
      stepper.next();
    }
  };

  const CTA_Content = {
    create: "Create",
    edit: "Update",
  };

  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Briefcase className="text-brand h-5 w-5" />
          Job Post Configuration
        </CardTitle>
      </CardHeader>

      <CardContent>
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleSubmit)}
            className="space-y-8"
          >
            {/* Step Navigation */}
            <nav aria-label="Job Post Creation Steps" className="group">
              <ol
                className="flex items-center justify-between gap-2"
                aria-orientation="horizontal"
              >
                {stepper.all.map((step, index, array) => (
                  <Fragment key={step.id}>
                    <li className="flex flex-shrink-0 items-center gap-4">
                      <Button
                        role="tab"
                        type="button"
                        variant={
                          index <= currentIndex ? "default" : "secondary"
                        }
                        disabled={index > currentIndex}
                        className="flex size-10 items-center justify-center rounded-full"
                        onClick={async () => {
                          const valid = await form.trigger();
                          // Must be validated
                          if (!valid) return;

                          // Can't skip steps forwards but can go back anywhere if validated
                          if (index - currentIndex > 1) return;
                          stepper.goTo(step.id);
                        }}
                        aria-current={
                          stepper.current.id === step.id ? "step" : undefined
                        }
                        aria-posinset={index + 1}
                        aria-setsize={steps.length}
                        aria-selected={stepper.current.id === step.id}
                      >
                        {index + 1}
                      </Button>

                      <span className="text-sm font-medium">{step.label}</span>
                    </li>

                    {index < array.length - 1 && (
                      <Separator
                        className={`flex-1 ${
                          index < currentIndex ? "bg-primary" : "bg-muted"
                        }`}
                      />
                    )}
                  </Fragment>
                ))}
              </ol>
            </nav>

            {/* Step Content */}
            {stepper.switch({
              jobPostDetails: () => <JobPostDetailsStep form={form} />,
              jobSteps: () => <JobStepsStep form={form} />,
              confirmation: () => <ConfirmationStep form={form} variant={variant} />,
            })}

            {/* Navigation Buttons */}
            <div className="flex items-center justify-end gap-4">
              <Button
                variant="outline"
                type="button"
                disabled={stepper.isFirst}
                onClick={stepper.prev}
              >
                Back
              </Button>

              <Button type="submit" isLoading={form.formState.isSubmitting}>
                {stepper.isLast ? CTA_Content[variant] : "Next"}
              </Button>
            </div>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}
