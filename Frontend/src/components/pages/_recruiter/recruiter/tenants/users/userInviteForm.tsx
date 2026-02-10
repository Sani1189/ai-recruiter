"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";

import BackButton from "@/components/ui/back-button";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";

import { TEMP } from "@/constants/temp";
import { useApi } from "@/hooks/useApi";
import { UserInviteSchema } from "@/schemas/tenants/user.schema";

export default function UserInviteForm() {
  const api = useApi(true);
  const router = useRouter();

  const [loading, setLoading] = useState(false);

  const form = useForm<UserInviteSchema>({
    resolver: zodResolver(UserInviteSchema),
    defaultValues: {
      email: "",
    },
  });

  const onSubmit = async (data: UserInviteSchema) => {
    try {
      setLoading(true);

      const response = await api.post("/TenantUsers/invite", {
        tenantId: TEMP.tenantId,
        email: data.email,
      });

      if (!response.success) {
        throw new Error(response.message);
      }

      toast.success("Invitation sent successfully!");
      router.push("/recruiter/tenants/users");
    } catch (error: any) {
      toast.error(
        error.message ||
          error?.response?.data?.message ||
          "Failed to send invitation.",
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-8">
      <div className="mx-auto max-w-md space-y-6">
        {/* Back */}
        <BackButton />

        {/* Header */}
        <div className="space-y-1">
          <h1 className="text-2xl font-bold">Invite User</h1>
          <p className="text-muted-foreground">
            Enter the email address to send an invitation
          </p>
        </div>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
            {/* Email */}
            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Email *</FormLabel>
                  <FormControl>
                    <Input
                      type="email"
                      placeholder="user@example.com"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Actions */}
            <div className="flex gap-4">
              <Button type="submit" disabled={loading}>
                {loading ? "Sending..." : "Send"}
              </Button>

              <Button
                type="button"
                variant="outline"
                onClick={() => router.back()}
              >
                Cancel
              </Button>
            </div>
          </form>
        </Form>
      </div>
    </div>
  );
}
