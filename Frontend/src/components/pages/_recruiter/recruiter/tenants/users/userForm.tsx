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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { TEMP } from "@/constants/temp";
import { useApi } from "@/hooks/useApi";
import { useServerTable } from "@/hooks/useServerTable";
import { PaginatedResponse } from "@/lib/api";
import { UserEditSchema } from "@/schemas/tenants/user.schema";
import { ConstantValueType } from "@/types/v2/type";

interface CreateUserFormProps {
  mode: "create";
}

interface EditUserFormProps {
  user: UserEditSchema;
  userId: string;
  mode: "edit";
}

type UserFormProps = CreateUserFormProps | EditUserFormProps;

const fetchData = async <T,>(
  api: ReturnType<typeof useApi>,
  endpoint: string,
): Promise<PaginatedResponse<T>> => {
  const response = await api.get<T[]>(endpoint);

  const pagination = {
    page: 1,
    pageSize: 10,
    totalItems: 50,
    totalPages: 5,
    hasNext: true,
    hasPrevious: false,
  };

  if (!response.success) {
    return {
      success: false,
      data: [],
      pagination,
    };
  }

  // Transform API response to match PaginatedResponse format
  return {
    success: true,
    data: response.data,
    pagination,
  };
};

export default function UserForm(props: UserFormProps) {
  const { mode } = props;

  const [loading, setLoading] = useState(false);

  const router = useRouter();
  const api = useApi(true);

  // Create API fetch function for useServerTable
  const fetchStatusCodes = async () =>
    fetchData<ConstantValueType>(api, `/Dropdown/statustypes`);

  const fetchRoleTypes = async () =>
    fetchData<ConstantValueType>(api, `/Dropdown/tenantroles`);

  const { data: statusCodes, isLoading: isStatusCodesLoading } =
    useServerTable(fetchStatusCodes);
  const { data: roleTypes, isLoading: isRoleTypesLoading } =
    useServerTable(fetchRoleTypes);

  const defaultValues: UserEditSchema =
    mode === "edit"
      ? props.user
      : {
          firstName: "",
          lastName: "",
          roleId: 0,
          statusCode: "INA",
        };

  const form = useForm<UserEditSchema>({
    resolver: zodResolver(UserEditSchema),
    defaultValues,
  });

  const onSubmit = async (data: UserEditSchema) => {
    const config = {
      endpoint:
        mode === "create"
          ? "/TenantUsers"
          : `/tenantusers/${TEMP.tenantId}/${props.userId}`,
      body:
        mode === "create"
          ? {
              ...data,
              tenantId: TEMP.tenantId,
              roleId: 2,
            }
          : data,
      method: mode === "create" ? api.post : api.put,
    };

    try {
      setLoading(true);
      const response = await config.method(config.endpoint, config.body);
      if (!response.success) {
        throw new Error(response.message);
      }

      toast.success(
        mode === "create"
          ? "User created and invitation sent!"
          : "User updated successfully!",
      );
      router.push("/recruiter/tenants/users");
    } catch (error: any) {
      toast.error(
        error.message ||
          error?.response?.data?.message ||
          "Failed to save user. Please try again.",
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-8">
      <div className="mx-auto max-w-2xl space-y-6">
        {/* Back Button */}
        <BackButton />

        {/* Header */}
        <div className="space-y-2">
          <h1 className="text-3xl font-bold">
            {mode === "create" ? "Create New User" : "Edit User"}
          </h1>
          <p className="text-muted-foreground">
            {mode === "create"
              ? "Create a new user for your recruitment process"
              : "Update the user details"}
          </p>
        </div>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
            {/* First Name */}
            <FormField
              control={form.control}
              name="firstName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>First Name *</FormLabel>
                  <FormControl>
                    <Input placeholder="John" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Last Name */}
            <FormField
              control={form.control}
              name="lastName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Last Name *</FormLabel>
                  <FormControl>
                    <Input placeholder="Doe" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-2 gap-3">
              {/* Role ID */}
              <FormField
                control={form.control}
                name="roleId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Role*</FormLabel>

                    <Select
                      defaultValue={field.value.toString()}
                      onValueChange={(value) => field.onChange(Number(value))}
                      disabled={isRoleTypesLoading}
                    >
                      <FormControl>
                        <SelectTrigger className="w-full">
                          <SelectValue placeholder="Select a role" />
                        </SelectTrigger>
                      </FormControl>

                      <SelectContent className="max-h-80 overflow-auto">
                        {roleTypes.map((item, index) => (
                          <SelectItem key={index} value={item.value.toString()}>
                            {item.text}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Status */}
              <FormField
                control={form.control}
                name="statusCode"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Status *</FormLabel>

                    <Select
                      defaultValue={field.value}
                      onValueChange={field.onChange}
                      disabled={isStatusCodesLoading}
                    >
                      <FormControl>
                        <SelectTrigger className="w-full">
                          <SelectValue placeholder="Select a status" />
                        </SelectTrigger>
                      </FormControl>

                      <SelectContent className="max-h-80 overflow-auto">
                        {statusCodes.map((item, index) => (
                          <SelectItem key={index} value={item.value}>
                            {item.value} - {item.text}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Actions */}
            <div className="flex gap-4">
              <Button type="submit" isLoading={loading}>
                {mode === "create" ? "Create User" : "Update User"}
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
