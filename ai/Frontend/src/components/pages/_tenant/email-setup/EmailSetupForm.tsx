"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { Save } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";

import BackButton from "@/components/ui/back-button";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";

import { useApi } from "@/hooks/useApi";
import {
  TenantEmailSettingSchema,
  TenantEmailSettingSchemaType,
  EmailSettingDefaultValues,
} from "@/schemas/tenants/email-setting.schema";

type EmailSetupCreateProps = {
  mode: "create";
  tenantId: string;
};

type EmailSetupEditProps = {
  mode: "edit";
  tenantId: string;
  emailSettingId: string;
  defaultValues: TenantEmailSettingSchemaType;
};

type EmailSetupFormProps = EmailSetupCreateProps | EmailSetupEditProps;

export default function EmailSetupForm(props: EmailSetupFormProps) {
  const { mode, tenantId } = props;
  const router = useRouter();
  const api = useApi(true);
  const [loading, setLoading] = useState(false);

  // Default values for form
  const defaultValues: TenantEmailSettingSchemaType =
    mode === "edit"
      ? props.defaultValues
      : { ...EmailSettingDefaultValues, tenantId };

  const form = useForm<TenantEmailSettingSchemaType>({
    resolver: zodResolver(TenantEmailSettingSchema),
    defaultValues,
  });

  const onSubmit = async (data: TenantEmailSettingSchemaType) => {
    setLoading(true);
    try {
      let endpoint = `/tenantemailsettings/${tenantId}`;
      let method = api.post;

      if (mode === "edit") {
        if (!("emailSettingId" in props) || !props.emailSettingId) {
          throw new Error("Email Setting ID is required for edit mode");
        }
        endpoint = `/tenantemailsettings/${props.emailSettingId}`;
        method = api.put;
      }

      const res = await method(endpoint, data);
      if (!res?.success) throw new Error(res?.message || "Failed to save email settings");

      toast.success(`Email settings ${mode === "edit" ? "updated" : "created"} successfully`);
      router.back();
    } catch (err: any) {
      toast.error(err.message || "Failed to save email settings");
    } finally {
      setLoading(false);
    }
  };

  // Text/number fields
  const textFields: (keyof TenantEmailSettingSchemaType)[] = [
    "providerName",
    "displayName",
    "fromEmail",
    "fromName",
    "host",
    "username",
    "passwordEncrypted",
    "apiKeyEncrypted",
  ];

  return (
    <div className="bg-muted/30 min-h-screen py-12">
      <div className="container px-20">
        {/* Header */}
        <div className="mb-8">
          <BackButton />
          <div>
            <h1 className="text-3xl font-bold">
              {mode === "create" ? "Create Email Setting" : "Edit Email Setting"}
            </h1>
            <p className="text-muted-foreground">
              {mode === "create"
                ? "Add a new tenant email setting"
                : "Update tenant email setting"}
            </p>
          </div>
        </div>

        <Card className="mx-auto">
          <CardHeader>
            <CardTitle>Email Settings</CardTitle>
            <CardDescription>Configure your tenant's email settings</CardDescription>
          </CardHeader>

          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8" noValidate>
              <CardContent className="space-y-4">
                {/* Provider Type */}
                <FormField
                  control={form.control}
                  name="providerType"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Provider Type</FormLabel>
                      <FormControl>
                        <select
                          {...field}
                          value={field.value}
                          className="border rounded p-2 w-full"
                        >
                          <option value="SMT">SMTP</option>
                          <option value="API">API</option>
                        </select>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Text & Number Inputs */}
                {textFields.map((name) => (
                  <FormField
                    key={name}
                    control={form.control}
                    name={name}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{name}</FormLabel>
                        <FormControl>
                          <Input
                            {...field}
                            value={
                              typeof field.value === "boolean" || field.value === null
                                ? ""
                                : field.value
                            }
                            onChange={(e) => field.onChange(e.target.value)}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                ))}

                {/* Port input */}
                <FormField
                  control={form.control}
                  name="port"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Port</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          {...field}
                          value={field.value ?? 587}
                          onChange={(e) => field.onChange(Number(e.target.value))}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Checkboxes */}
                {["enableSsl", "useStartTls", "isActive", "isDefault"].map((name) => (
                  <FormField
                    key={name}
                    control={form.control}
                    name={name as keyof TenantEmailSettingSchemaType}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{name}</FormLabel>
                        <FormControl>
                          <Checkbox
                            checked={!!field.value}
                            onCheckedChange={(val) => field.onChange(!!val)}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                ))}
              </CardContent>

              <CardFooter className="justify-end gap-3">
                <Button variant="ghost" type="button" onClick={() => router.back()}>
                  Cancel
                </Button>
                <Button type="submit" isLoading={loading}>
                  <Save className="mr-2 h-4 w-4" />
                  {mode === "edit" ? "Update" : "Create"}
                </Button>
              </CardFooter>
            </form>
          </Form>
        </Card>
      </div>
    </div>
  );
}
