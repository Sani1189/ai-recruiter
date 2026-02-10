"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { Building2, Save } from "lucide-react";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";

import { revalidatePath } from "@/actions/revalidatePath";
import { useApi } from "@/hooks/useApi";
import { useServerTable } from "@/hooks/useServerTable";
import { PaginatedResponse } from "@/lib/api";
import { TenantSchema } from "@/schemas/tenants/tenant.schema";
import { ConstantValueType } from "@/types/v2/type";

type TenantProfileEditFormProps = {
  mode: "edit";
  tenant: TenantSchema;
  tenantId: string;
};

type TenantProfileCreateFormProps = {
  mode: "create";
};

type TenantProfileFormProps =
  | TenantProfileEditFormProps
  | TenantProfileCreateFormProps;

export default function TenantProfileForm(props: TenantProfileFormProps) {
  const { mode } = props;

  const router = useRouter();
  const api = useApi(true);

  const [loading, setLoading] = useState(false);

  // Create API fetch function for useServerTable
  const fetchUsers = async (): Promise<
    PaginatedResponse<ConstantValueType>
  > => {
    const response = await api.get<ConstantValueType[]>(`/Dropdown/countries`);

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

  // Use server table hook for pagination, filtering, and sorting
  const { data: countries, isLoading: isCountriesLoading } =
    useServerTable(fetchUsers);

  const defaultValues: TenantSchema =
    mode === "edit"
      ? props.tenant
      : {
          tenantName: "",
          subDomain: "",
          email: "",
          web: "",
          mobile: "",
          address1: "",
          address2: "",
          city: "",
          state: "",
          zip: "",
          country: "",
        };

  const form = useForm<TenantSchema>({
    resolver: zodResolver(TenantSchema),
    defaultValues,
  });

  const onSubmit = async (data: TenantSchema) => {
    try {
      setLoading(true);

      const config = {
        endpoint: mode === "create" ? "/tenants" : `/tenants/${props.tenantId}`,
        method: mode === "create" ? api.post : api.put,
      };

      const res = await config.method(config.endpoint, data);
      if (!res?.success) {
        throw new Error(res?.message || "Failed to save tenant");
      }

      toast.success(
        `Tenant ${mode === "create" ? "created" : "updated"} successfully`,
      );

      // Revalidate the tenants profiles page
      await revalidatePath("/recruiter/tenants/profiles");

      router.back();
    } catch (error: any) {
      toast.error(error.message || "Failed to save tenant");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-muted/30 min-h-screen py-12">
      <div className="container px-20">
        {/* Header */}
        <div className="mb-8">
          <BackButton />

          <div>
            <h1 className="text-3xl font-bold">
              {mode === "create" ? "Create Tenant" : "Edit Tenant"}
            </h1>
            <p className="text-muted-foreground">
              {mode === "create"
                ? "Add a new tenant"
                : "Update tenant information"}
            </p>
          </div>
        </div>

        <Card className="mx-auto">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              Tenant Profile
            </CardTitle>

            <CardDescription>Tenant information</CardDescription>
          </CardHeader>

          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
              <CardContent className="space-y-8">
                {/* Basic Info */}
                <section className="space-y-4">
                  <h3 className="text-lg font-semibold">Basic Information</h3>

                  <div className="grid gap-4 md:grid-cols-2">
                    {/* Tenant Name */}
                    <FormField
                      control={form.control}
                      name="tenantName"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Tenant Name *</FormLabel>
                          <FormControl>
                            <Input placeholder="Name" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    {/* SubDomain */}
                    <FormField
                      control={form.control}
                      name="subDomain"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>SubDomain *</FormLabel>
                          <FormControl>
                            <Input
                              placeholder="Enter subdomain"
                              {...field}
                              disabled={mode === "edit"}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </section>

                <Separator />

                {/* Contact */}
                <section className="space-y-4">
                  <h3 className="text-lg font-semibold">Contact</h3>

                  <div className="grid gap-4 md:grid-cols-2">
                    {/* Email */}
                    <FormField
                      control={form.control}
                      name="email"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Email *</FormLabel>
                          <FormControl>
                            <Input
                              placeholder="email@example.com"
                              type="email"
                              {...field}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    {/* Web */}
                    <FormField
                      control={form.control}
                      name="web"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Web *</FormLabel>
                          <FormControl>
                            <Input
                              placeholder="https://example.com"
                              {...field}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    {/* Mobile */}
                    <FormField
                      control={form.control}
                      name="mobile"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Mobile *</FormLabel>
                          <FormControl>
                            <Input
                              placeholder="Enter mobile number"
                              {...field}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </section>

                <Separator />

                {/* Address */}
                <section className="space-y-4">
                  <h3 className="text-lg font-semibold">Address</h3>

                  {/* Address 1 */}
                  <FormField
                    control={form.control}
                    name="address1"
                    render={({ field }) => (
                      <FormItem>
                        <FormControl>
                          <Input placeholder="Address line 1" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Address 2 */}
                  <FormField
                    control={form.control}
                    name="address2"
                    render={({ field }) => (
                      <FormItem>
                        <FormControl>
                          <Input placeholder="Address line 2" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <div className="grid gap-4 md:grid-cols-2">
                    {/* City */}
                    <FormField
                      control={form.control}
                      name="city"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>City *</FormLabel>
                          <FormControl>
                            <Input placeholder="Enter city" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    {/* State */}
                    <FormField
                      control={form.control}
                      name="state"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>State *</FormLabel>
                          <FormControl>
                            <Input placeholder="Enter state" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    {/* ZIP */}
                    <FormField
                      control={form.control}
                      name="zip"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>ZIP *</FormLabel>
                          <FormControl>
                            <Input placeholder="Enter ZIP" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    {/* Country Dropdown */}
                    <FormField
                      control={form.control}
                      name="country"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Country *</FormLabel>
                          <Select
                            defaultValue={field.value}
                            onValueChange={field.onChange}
                            disabled={isCountriesLoading}
                          >
                            <FormControl>
                              <SelectTrigger className="w-full">
                                <SelectValue placeholder="Select a country" />
                              </SelectTrigger>
                            </FormControl>

                            <SelectContent className="max-h-80 overflow-auto">
                              {countries.map((country) => (
                                <SelectItem
                                  key={country.value}
                                  value={country.value}
                                >
                                  {country.text}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </section>
              </CardContent>

              <CardFooter className="justify-end gap-3">
                <Button
                  variant="ghost"
                  type="button"
                  onClick={() => router.back()}
                >
                  Cancel
                </Button>

                <Button type="submit" isLoading={loading}>
                  <Save className="mr-2 h-4 w-4" />
                  {mode === "create" ? "Create Tenant" : "Update Tenant"}
                </Button>
              </CardFooter>
            </form>
          </Form>
        </Card>
      </div>
    </div>
  );
}
